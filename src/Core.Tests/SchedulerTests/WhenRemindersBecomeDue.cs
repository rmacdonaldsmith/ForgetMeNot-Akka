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
            var now = SystemTime.FreezeTime().ToUniversalTime();
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler");

            scheduler.Tell(new SystemMessage.Start());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now - 1.Hours()));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now - 1.Milliseconds()));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now + 4.Minutes()));
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage(now + 2.Hours()));

            //advance time, check for reminders
            SystemTime.Set(now + 1.Seconds());
            scheduler.Tell(new Scheduler.Messages.CheckQueue());

            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();
            ExpectMsg<ReminderMessage.Due>();      
      
            //ask the Schduler how many items are in the queue
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());
            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(query => query.Size.Should().Be(2));

            //advance time, check for reminders
            SystemTime.Set(now + 5.Minutes());
            scheduler.Tell(new Scheduler.Messages.CheckQueue());
            ExpectMsg<ReminderMessage.Due>();

            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());
            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(query => query.Size.Should().Be(1));

            //advance time, check for reminders
            SystemTime.Set(now + 2.Hours());
            scheduler.Tell(new Scheduler.Messages.CheckQueue());
            ExpectMsg<ReminderMessage.Due>();

            //we should have exhuasted the queue
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());
            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(query => query.Size.Should().Be(0));
        }
    }
}
