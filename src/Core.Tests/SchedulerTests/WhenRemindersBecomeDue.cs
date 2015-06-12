using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.SchedulerTests
{
    [TestFixture]
    public class WhenRemindersBecomeDue : TestKit
    {
        public void ThenDueMessageIsSent()
        {
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler");

            scheduler.Tell(new SystemMessage.Start());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage());

            ExpectMsg<ReminderMessage.Due>(TimeSpan.FromMilliseconds(100));
        }
    }
}
