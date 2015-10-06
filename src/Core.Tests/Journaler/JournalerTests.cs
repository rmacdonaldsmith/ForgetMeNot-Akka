using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Core.Journaler;
using ForgetMeNot.Messages;
using NUnit.Framework;
using JournalerActor = ForgetMeNot.Core.Journaler.Journaler;

namespace ForgetMeNot.Core.Tests.Journaler
{
    public class JournalerTests : TestKit
    {
        [Test]
        public void Should_respond_when_message_has_been_journaled()
        {
            Task.Run(async () =>
                {
                    var journalWriter = new FakeJournaler();
                    var journaler = ActorOf(JournalerActor.PropsFactory(journalWriter));
                    var reminderId = Guid.NewGuid();

                    await
                        journaler.Ask<JournalerActor.Messages.Journaled<ReminderMessage.Cancel>>(
                            new ReminderMessage.Cancel(reminderId));

                    ExpectMsg<JournalerActor.Messages.Journaled<ReminderMessage.Cancel>>(
                        journaled => journaled.Message.ReminderId == reminderId);
                });
        }

        [Test]
        public void Should_receive_a_failure_when_not_journaled()
        {
            Task.Run(async () =>
            {
                var journalWriter = new FakeJournaler(msg => { throw new Exception(); });
                var journaler = ActorOf(JournalerActor.PropsFactory(journalWriter));
                var reminderId = Guid.NewGuid();

                await
                    journaler.Ask<JournalerActor.Messages.Journaled<ReminderMessage.Cancel>>(
                        new ReminderMessage.Cancel(reminderId));

                ExpectMsg<Failure>();
            });
        }
    }

    public class FakeJournaler : IJournalEvents
    {
        private readonly Action<object> _writeDelegate = msg => { };

        public FakeJournaler()
        {
            //empty
        }

        public FakeJournaler(Action<object> writeDelegate)
        {
            _writeDelegate = writeDelegate;
        }

        public void Write(object message)
        {
            _writeDelegate(message);
        }
    }
}
