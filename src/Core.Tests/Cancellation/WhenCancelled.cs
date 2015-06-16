using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using ForgetMeNot.Core.Cancellation;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.Cancellation
{
    public class WhenCancelled : TestKit
    {
        [Test]
        public void ShouldAllowNonCancelledRemindersThrough()
        {
            var cancellationFilter = ActorOf(CancellationFilter.ActorProps(TestActor), "cancellation-filter-1");
            cancellationFilter.Tell(new ReminderMessage.Cancel(Guid.NewGuid()));
            cancellationFilter.Tell(new ReminderMessage.Cancel(Guid.NewGuid()));

            var due = new ReminderMessage.Due(TestHelper.BuildMeAScheduleMessage());
            cancellationFilter.Tell(due);

            ExpectMsg<ReminderMessage.Due>(dueReminder => due.ReminderId = dueReminder.ReminderId);
        }

        [Test]
        public void ShouldBlockCancelledReminders()
        {
            var cancelledReminderId = Guid.NewGuid();
            var cancellationFilter = ActorOf(CancellationFilter.ActorProps(TestActor), "cancellation-filter-2");
            cancellationFilter.Tell(new ReminderMessage.Cancel(cancelledReminderId));
            cancellationFilter.Tell(new ReminderMessage.Cancel(Guid.NewGuid()));

            var due = new ReminderMessage.Due(TestHelper.BuildMeAScheduleMessage().WithIdentity(cancelledReminderId));
            cancellationFilter.Tell(due);

            ExpectNoMsg();

            //we should now be able to send the due reminder with this "cancelled" id again
            //it should have been removed from the list that the CancellationFilter maintains,
            //hence we can test that the list is being maintained
            cancellationFilter.Tell(due);

            ExpectMsg<ReminderMessage.Due>(dueReminder => due.ReminderId == dueReminder.ReminderId);
        }
    }
}
