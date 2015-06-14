using System;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public enum DeliveryTransport
	{
		None,
		Http,
		RabbitMq
	}

	public class DeliveryRouter : TypedActor, 
        IHandle<ReminderMessage.Due>,
        IHandle<DeliveryMessage.Undeliverable>,
        IHandle<DeliveryMessage.Rescheduled>
	{
	    private readonly IActorRef _journaler;
	    private readonly IActorRef _httpDelivery;
	    private readonly IActorRef _deadletterDelivery;
	    private readonly ILog Logger = LogManager.GetLogger(typeof(DeliveryRouter));

	    public DeliveryRouter(IActorRef journaler,Props httpDeliveryProps, Props deadletterDeliveryProps)
	    {
            Ensure.NotNull(journaler, "journaler");
	        Ensure.NotNull(httpDeliveryProps, "httpDeliveryProps");
            Ensure.NotNull(deadletterDeliveryProps, "deadletterDeliveryProps");

	        _journaler = journaler;
            _httpDelivery = Context.ActorOf(httpDeliveryProps);
	        _deadletterDelivery = Context.ActorOf(deadletterDeliveryProps);
	    }

	    public void Handle(ReminderMessage.Due due)
	    {
	        if (due.Reminder.Transport == ReminderMessage.TransportEnum.http)
	            _httpDelivery.Tell(due);
	        else
	            Logger.ErrorFormatNoException(
	                new {logname = "delivery", disposition = "undeliverable", reminderid = due.ReminderId},
	                "There are no reminder delivery handlers registered for transport [{0}], delivery url [{1}]",
	                due.Reminder.Transport, due.Reminder.DeliveryUrl);
	    }

        public void Handle(DeliveryMessage.Undeliverable message)
        {
            _deadletterDelivery.Tell(message);
        }

	    public void Handle(DeliveryMessage.Rescheduled reschedule)
	    {
            _journaler.Tell(reschedule);
	    }

	    protected override SupervisorStrategy SupervisorStrategy()
	    {
            //hmmm, this is looking awfully like the UndeliverableProcessManager
	        return new OneForOneStrategy(
	            maxNrOfRetries: 3,
	            withinTimeMilliseconds: 1000,
	            localOnlyDecider: exception => Directive.Restart, 
	            loggingEnabled: true
	            );
	    }

	    public static Func<IActorRef, Props, Props, Props> PropsFactory
	    {
            get
            {
                return
                    (journaler, httpDeliveryProps, deadletterDeliveryProps) =>
                    Props.Create(() => new DeliveryRouter(journaler, httpDeliveryProps, deadletterDeliveryProps));
            }
	    }
	}
}