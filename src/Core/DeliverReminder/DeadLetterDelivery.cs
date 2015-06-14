using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class DeadLetterDelivery : IConsume<ReminderMessage.Undeliverable>
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DeadLetterDelivery));
		private readonly ISendMessages _bus;
		private readonly IDeliverReminders _sender;
		private readonly string _deadLetterUrl;

		//For now, we will just deliver the Reminder to the DeadLetter location. We may want to send the Reminder wrapped in some structure
		//that exposes what went wrong with delivery. Not going to worry about that now, can do that later.
		public DeadLetterDelivery (ISendMessages bus, IDeliverReminders deadLetterSender, string deadLetterUrl)
		{
			Ensure.NotNull (bus, "bus");
			Ensure.NotNull (deadLetterSender, "deadLetterSender");
			Ensure.NotNullOrEmpty (deadLetterUrl, "deadLetterUrl");

			_bus = bus;
			_sender = deadLetterSender;
			_deadLetterUrl = deadLetterUrl;
		}

		public void Handle (ReminderMessage.Undeliverable undeliverable)
		{
			Logger.WarnFormat (
				new { logname = "delivery", disposition = "deadletter", reminderid =  undeliverable.ReminderId},
				"Reminder [{0}] was undelivereable. This log message is a stand-in for the dead-letter queue until we can get the dead-letter queue setup.", undeliverable.ReminderId);
			return;

			//check that the transport scheme of the url matches the transport of the delivery interface instance??
			_sender.Send(undeliverable.Reminder, _deadLetterUrl, OnSuccessfulDelivery, OnFailedDelivery);
			_bus.Send(new ReminderMessage.SentToDeadLetter(undeliverable.ReminderId, SystemTime.UtcNow()));

			Logger.InfoFormat(
				new { logname = "delivery", disposition = "deadletter", reminderid =  undeliverable.ReminderId },
				"Undeliverable reminder [{0}] sent to the deadletter location at [{0]}", 
				undeliverable.ReminderId, _deadLetterUrl);
		}

		private void OnSuccessfulDelivery(ReminderMessage.Schedule sentReminder)
		{
			_bus.Send (new ReminderMessage.SentToDeadLetter(sentReminder.ReminderId, SystemTime.UtcNow()));
		}

		private void OnFailedDelivery(ReminderMessage.Schedule failedReminder, string errorMessage)
		{
			var message = string.Format (
				"Unable to deliver Undeliverable reminder [{0}] to the deadletter location [{1}]. Reason: {2}", 
				failedReminder.ReminderId, _deadLetterUrl, errorMessage);
			Logger.ErrorFormatNoException(new { logname = "delivery", disposition = "undeliverable", reminderid = failedReminder.ReminderId },
				message);
			throw new ReminderUndeliverableException<ReminderMessage.Schedule> (failedReminder, message);
		}
	} 
}

