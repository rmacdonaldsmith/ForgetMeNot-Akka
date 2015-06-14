using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Core.Schedule;
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

        [Test(Description = 
            "Does not start processing messages until Start is received. Stops processing the q when ShutDown is received.")]
        public void StartAndShutdown()
        {
            var schedulerProps = Props.Create(() => new Scheduler(10, TestActor));
            var scheduler = ActorOf(schedulerProps, "scheduler-2");

            scheduler.Tell(new SystemMessage.Start());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage());
            scheduler.Tell(new QueryMessage.HowBigIsYourQueue());

            ExpectMsg<QueryMessage.HowBigIsYourQueueResponse>(msg => msg.Size == 1);

            //does not accept schedule messages when ShutDown has been received
            scheduler.Tell(new SystemMessage.ShutDown());
            scheduler.Tell(TestHelper.BuildMeAScheduleMessage());
            scheduler.Tell(new Scheduler.Messages.CheckQueue());

            ExpectNoMsg(500);
        }
    }
}
