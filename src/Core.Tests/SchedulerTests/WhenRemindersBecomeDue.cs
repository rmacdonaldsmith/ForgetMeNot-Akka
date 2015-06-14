using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Common;
using ForgetMeNot.Core.Schedule;
using ForgetMeNot.Messages;
using NUnit.Framework;
using FluentAssertions;

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
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now + 4.Minutes()));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now + 2.Hours()));

            SystemTime.Set(now + 1.Seconds());

            scheduler.Tell(new Scheduler.Messages.CheckQueue());

            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();      
      
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());

            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(query => query.Size.Should().Be(2));
        }
    }
}
