using System;
using System.Collections.Generic;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class UndeliveredProcessManager : 
		IConsume<ReminderMessage.Undelivered>,
		IConsume<ReminderMessage.Delivered>
	{
		private readonly ILog Logger = LogManager.GetLogger(typeof(UndeliveredProcessManager));
		private readonly object _lockObject = new object ();
		private readonly IBus _bus;
		private readonly Dictionary<Guid, int> _retryAttempts = new Dictionary<Guid, int> ();

		public UndeliveredProcessManager (IBus bus)
		{
			Ensure.NotNull (bus, "bus");

			_bus = bus;
		}

		public void Handle (ReminderMessage.Delivered delivered)
		{
			lock (_lockObject) {
				if (_retryAttempts.ContainsKey (delivered.ReminderId)) {
					_retryAttempts.Remove (delivered.ReminderId);
					Logger.InfoFormat(new { logname = "delivery", disposition = "delivered", reminderid = delivered.ReminderId },
						"Reminder [{0}] was delivered, removing from cache.", delivered.ReminderId);
				}
			}
		}

		public void Handle (ReminderMessage.Undelivered undelivered)
		{
			Logger.InfoFormat ("Reminder [{0}] was undelivered...", undelivered.ReminderId);
			if (undelivered.DoNotAttemptRedelivery ()) {
				Logger.ErrorFormatNoException(new { logname = "delivery", disposition = "undeliverable", reminderid = undelivered.ReminderId },
					"Reminder [{0}] should not attempt redelivery; sending Undeliverable.", undelivered.ReminderId);
				_bus.Send (new ReminderMessage.Undeliverable (undelivered.Reminder, undelivered.Reason));
				return;
			}

			lock (_lockObject) {
				if (!_retryAttempts.ContainsKey (undelivered.ReminderId))
					_retryAttempts.Add (undelivered.ReminderId, 1);
				else
					_retryAttempts [undelivered.ReminderId]++;

				var retryAttempts = _retryAttempts [undelivered.ReminderId];
				var nextDueTime = CalculateNextDueTime (undelivered.Reminder, retryAttempts);
				var rescheduled = new ReminderMessage.Rescheduled (undelivered.Reminder, nextDueTime);

				if (GiveupRedelivering (rescheduled, retryAttempts)) {
					Logger.ErrorFormatNoException(new { logname = "delivery", disposition = "undeliverable", reminderid = undelivered.ReminderId },
						"Reminder [{0}] giving-up redelivery after [{1}] attempts; sending Undeliverable.", undelivered.ReminderId, retryAttempts);
					_bus.Send (new ReminderMessage.Undeliverable (undelivered.Reminder, undelivered.Reason));
				} else {
					Logger.InfoFormat(new { logname = "delivery", disposition = "retrying", reminderid = undelivered.ReminderId },
						"Reminder [{0}]: rescheduling delivery for [{1}].", undelivered.ReminderId, rescheduled.DueAt);
					_bus.Send (rescheduled);
				}
			}
		}
			
		private DateTime CalculateNextDueTime(ReminderMessage.Schedule reminder, int retryCount)
		{
			return reminder.DueAt.AddTicks (DecelerationFactor (reminder) * retryCount * retryCount * retryCount);
		}

		private long DecelerationFactor(ReminderMessage.Schedule reminder)
		{
			return (long)( (reminder.GiveupAfter.Value.Ticks - reminder.DueAt.Ticks) / (reminder.MaxRetries * reminder.MaxRetries * reminder.MaxRetries) );
		}

		private bool GiveupRedelivering(ReminderMessage.Rescheduled rescheduled, int retryAttempts)
		{
			return retryAttempts > rescheduled.Reminder.MaxRetries;
		}
	}
}