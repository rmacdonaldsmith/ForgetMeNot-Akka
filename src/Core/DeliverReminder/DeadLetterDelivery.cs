using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class DeadLetterDelivery : TypedActor, IHandle<DeliveryMessage.Undeliverable>
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DeadLetterDelivery));

		public void Handle (DeliveryMessage.Undeliverable undeliverable)
		{
		    Logger.WarnFormat(
		        new {logname = "delivery", disposition = "deadletter", reminderid = undeliverable.ReminderId},
		        "Reminder [{0}] was undelivereable. This log message is a stand-in for the dead-letter queue until that queue is setup.",
		        undeliverable.ReminderId);
		}
	} 
}

