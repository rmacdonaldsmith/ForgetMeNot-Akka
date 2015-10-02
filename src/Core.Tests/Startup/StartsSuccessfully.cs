using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.Startup
{
    public class StartsSuccessfully : TestKit
    {
        [Test]
        public void ShouldIssueInitCompletedMessage()
        {
            var cancellationReplayer = new FakeReplayer(from => this.BuildCancelEventStream(5));
            var currentRemindersReplayer = new FakeReplayer(from => this.BuildCurrentReminderStream(5));
            var startupManager = ActorOf(SystemStartManager.ActorProps(new List<IReplayEvents> { cancellationReplayer, currentRemindersReplayer }, TestActor), "ShouldIssueInitCompletedMessage");

            startupManager.Tell(new SystemMessage.BeginInitialization());

            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<ReminderMessage.IReminder>();
            ExpectMsg<SystemMessage.InitializationCompleted>();
        }

        private IObservable<ReminderMessage.IReminder> BuildCancelEventStream(int count)
        {
            return Observable
                .Range(0, count)
                .Select(x => new ReminderMessage.Cancel(Guid.NewGuid()))
                .Cast<ReminderMessage.IReminder>();
        }

        private IObservable<ReminderMessage.IReminder> BuildCurrentReminderStream(int count)
        {
            return Observable
                .Range(0, count)
                .Select(x => new ReminderMessage.Schedule())
                .Cast<ReminderMessage.IReminder>();
        }
    }
}
