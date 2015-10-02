using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.TestKit.NUnit;
using FluentAssertions;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.Startup
{
    public class WhenStartupFails : TestKit
    {
        [Test]
        public void ShouldNotSendInitializationCompleted()
        {
            var cancellationReplayer = new FakeReplayer(from =>
                Observable
                    .Range(0, 5)
                    .Select(x => new ReminderMessage.Cancel(Guid.NewGuid()))
                    .Cast<ReminderMessage.IReminder>());

            var currentRemindersReplayer = new FakeReplayer(from =>
                Observable.Throw<ReminderMessage.Schedule>(new Exception("Something went wrong in the database")));

            var startupManager = ActorOf(SystemStartManager.ActorProps(new List<IReplayEvents> {cancellationReplayer, currentRemindersReplayer}, TestActor), "system-start-manager-test");
            startupManager.Tell(new SystemMessage.BeginInitialization());

            ExpectMsg<SystemMessage.InitializationFailed>(msg => msg.Message.Should().Be("There was an exception while replaying data in to the system"));
        }
    }
}
