using System;
using System.Collections.Generic;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class UndeliveredProcessManager : TypedActor,
		IHandle<DeliveryMessage.NotDelivered>,
		IHandle<DeliveryMessage.Delivered>,
        IHandle<QueryMessage.HowManyUndeliveredRemindersDoYouHave>
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(UndeliveredProcessManager));
		private readonly Dictionary<Guid, int> _retryAttempts = new Dictionary<Guid, int> ();
	    private readonly IActorRef _deliveryRouter;

        public UndeliveredProcessManager(IActorRef deliveryRouter)
		{
            Ensure.NotNull(deliveryRouter, "deliveryRouter");

            _deliveryRouter = deliveryRouter;
		}

		public void Handle (DeliveryMessage.Delivered delivered)
		{
		    if (_retryAttempts.ContainsKey(delivered.ReminderId))
		    {
		        _retryAttempts.Remove(delivered.ReminderId);
		        Logger.InfoFormat(new {logname = "delivery", disposition = "delivered", reminderid = delivered.ReminderId},
		                          "Reminder [{0}] was delivered, removing from cache.", delivered.ReminderId);
		    }
		}

		public void Handle (DeliveryMessage.NotDelivered notDelivered)
		{
			Logger.InfoFormat ("Reminder [{0}] was undelivered...", notDelivered.ReminderId);
			if (notDelivered.DoNotAttemptRedelivery ()) {
				Logger.ErrorFormatNoException(new { logname = "delivery", disposition = "undeliverable", reminderid = notDelivered.ReminderId },
					"Reminder [{0}] should not attempt redelivery; sending Undeliverable.", notDelivered.ReminderId);
				_deliveryRouter.Tell(new DeliveryMessage.Undeliverable (notDelivered.Reminder, notDelivered.Reason));
				return;
			}

		    if (!_retryAttempts.ContainsKey(notDelivered.ReminderId))
		        _retryAttempts.Add(notDelivered.ReminderId, 1);
		    else
		        _retryAttempts[notDelivered.ReminderId]++;

		    var retryAttempts = _retryAttempts[notDelivered.ReminderId];
		    var nextDueTime = CalculateNextDueTime(notDelivered.Reminder, retryAttempts);
		    var rescheduled = new DeliveryMessage.Rescheduled(notDelivered.Reminder, nextDueTime);

		    if (GiveupRedelivering(rescheduled, retryAttempts))
		    {
		        Logger.ErrorFormatNoException(
		            new {logname = "delivery", disposition = "undeliverable", reminderid = notDelivered.ReminderId},
		            "Reminder [{0}] giving-up redelivery after [{1}] attempts; sending Undeliverable.",
		            notDelivered.ReminderId, retryAttempts);
                _deliveryRouter.Tell(new DeliveryMessage.Undeliverable(notDelivered.Reminder, notDelivered.Reason));
		    }
		    else
		    {
		        Logger.InfoFormat(new {logname = "delivery", disposition = "retrying", reminderid = notDelivered.ReminderId},
		                          "Reminder [{0}]: rescheduling delivery for [{1}].", notDelivered.ReminderId,
		                          rescheduled.DueAt);
                _deliveryRouter.Tell(rescheduled);
		    }

		}

        public void Handle(QueryMessage.HowManyUndeliveredRemindersDoYouHave message)
        {
            Sender.Tell(new QueryMessage.HowManyUndeliveredRemindersDoYouHaveResponse(_retryAttempts.Count));
        }

		private DateTime CalculateNextDueTime(ReminderMessage.Schedule reminder, int retryCount)
		{
			return reminder.DueAt.AddTicks (DecelerationFactor (reminder) * retryCount * retryCount * retryCount);
		}

		private long DecelerationFactor(ReminderMessage.Schedule reminder)
		{
			return (long)( (reminder.GiveupAfter.Value.Ticks - reminder.DueAt.Ticks) / (reminder.MaxRetries * reminder.MaxRetries * reminder.MaxRetries) );
		}

		private bool GiveupRedelivering(DeliveryMessage.Rescheduled rescheduled, int retryAttempts)
		{
			return retryAttempts > rescheduled.Reminder.MaxRetries;
		}

	    public static Func<IActorRef, Props> ActorProps
	    {
	        get { return deliveryRouter => Props.Create(() => new UndeliveredProcessManager(deliveryRouter)); }
	    }
	}
}