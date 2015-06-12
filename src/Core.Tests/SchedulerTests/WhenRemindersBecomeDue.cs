using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.SchedulerTests
{
    [TestFixture]
    public class WhenRemindersBecomeDue : TestKit
    {
        [Test]
        public void ThenDueMessagesAreSent()
        {
            var now = SystemTime.FreezeTime();
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler");

            scheduler.Tell(new SystemMessage.Start());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));

            SystemTime.Set(now + TimeSpan.FromSeconds(1));

            scheduler.Tell(new Scheduler.Messages.CheckQueue());

            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();            
        }
    }
}
