using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Journaler
{
    public class Journaler : TypedActor,
                             IHandle<ReminderMessage.Schedule>,
                             IHandle<ReminderMessage.Cancel>,
                             IHandle<DeliveryMessage.Delivered>,
                             IHandle<DeliveryMessage.Undeliverable>
    {
        private readonly IJournalEvents _journaler;
        private readonly IActorRef _scheduler;

        public Journaler(IJournalEvents journaler, IActorRef scheduler)
        {
            Ensure.NotNull(journaler, "journaler");
            Ensure.NotNull(scheduler, "scheduler");

            _journaler = journaler;
            _scheduler = scheduler;
        }

        public void Handle(ReminderMessage.Schedule message)
        {
            WriteToJournal(message);
        }

        public void Handle(ReminderMessage.Cancel message)
        {
            WriteToJournal(message);
        }

        public void Handle(DeliveryMessage.Delivered message)
        {
            WriteToJournal(message);
        }

        public void Handle(DeliveryMessage.Undeliverable message)
        {
            WriteToJournal(message);
        }

        private void WriteToJournal(object message)
        {
            _journaler.Write(message);
            _scheduler.Tell(message);
        }
    }
}
