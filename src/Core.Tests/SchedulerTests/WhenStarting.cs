using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.SchedulerTests
{
    [TestFixture]
    public class WhenStarting : TestKit
    {
        [Test]
        public void DoesNotRunUntilStartIsReceived()
        {
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler-1");

            scheduler.Tell(TestHelper.BuildMeAScheduleMessage());
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());

            ExpectNoMsg(1100);
        }

        [Test]
        public void StartsWhenStartIsReceived()
        {
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler-2");

            scheduler.Tell(new SystemMessage.Start());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage());
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());

            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(msg => msg.Size == 1);
        }
    }
}
