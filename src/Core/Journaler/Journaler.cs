using System;
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

        public Journaler(IJournalEvents journaler)
        {
            Ensure.NotNull(journaler, "journaler");

            _journaler = journaler;
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

        private void WriteToJournal<T>(T message)
        {
            try
            {
                _journaler.Write(message);
                Sender.Tell(new Messages.Journalled<T>(message), Self);
            }
            catch (Exception e)
            {
                Sender.Tell(new Failure {Exception = e}, Self);
                throw;
            }
        }

        public static Func<IJournalEvents, Props> PropsFactory
        {
            get { return ijournalevents => Props.Create(() => new Journaler(ijournalevents)); }
        }

        public class Messages
        {
            public class Journalled<T>
            {
                public Journalled(T message)
                {
                    Message = message;
                }

                public T Message { get; private set; }
            }
        }
    }
}
