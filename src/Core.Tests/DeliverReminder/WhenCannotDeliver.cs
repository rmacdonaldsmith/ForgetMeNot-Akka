using Akka.Actor;
using Akka.TestKit.NUnit;
using FluentAssertions;
using ForgetMeNot.Common;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.DeliverReminder
{
    public class WhenCannotDeliver : TestKit
    {
        [Test]
        public void WhenRedeliveryShouldNotBeAttempted()
        {
            var now = SystemTime.FreezeTime();
            var undeliveredProcessManager = ActorOf(Props.Create(() => new UndeliveredProcessManager(TestActor)),
                                                    "undelivered-process-manager-1");

            var reminderMessage = TestHelper.BuildMeAScheduleMessage(now);
            undeliveredProcessManager.Tell(new DeliveryMessage.NotDelivered(reminderMessage, "a reason"));

            ExpectMsg<DeliveryMessage.Undeliverable>();
        }

        [Test]
        public void WhenRetryingButUndeliverable()
        {
            var now = SystemTime.FreezeTime();
            var undeliveredProcessManager = ActorOf(Props.Create(() => new UndeliveredProcessManager(TestActor)),
                                                    "undelivered-process-manager-2");

            var reminderMessage = TestHelper
                .BuildMeAScheduleMessage(now)
                .WithRetry(
                    attempts: 3, 
                    retryPeriod: 10.Minutes ());
            var notDelivered = new DeliveryMessage.NotDelivered(reminderMessage, "a reason");
            
            undeliveredProcessManager.Tell(notDelivered);
            ExpectMsg<DeliveryMessage.Rescheduled>();
            undeliveredProcessManager.Tell(notDelivered);
            ExpectMsg<DeliveryMessage.Rescheduled>();
            undeliveredProcessManager.Tell(notDelivered);
            ExpectMsg<DeliveryMessage.Rescheduled>(reschedule =>
                                                   Assert.That(reschedule.DueAt,
                                                               Is.InRange(now + 9.Minutes(), now + 10.Minutes())));
            undeliveredProcessManager.Tell(notDelivered);
            ExpectMsg<DeliveryMessage.Undeliverable>();
        }

        [Test]
        public void WhenRetryingAndSucceed()
        {
            var now = SystemTime.FreezeTime();
            var undeliveredProcessManager = ActorOf(Props.Create(() => new UndeliveredProcessManager(TestActor)),
                                                    "undelivered-process-manager-3");

            var reminderMessage = TestHelper
                .BuildMeAScheduleMessage(now)
                .WithRetry(
                    attempts: 3,
                    retryPeriod: 10.Minutes());
            var notDelivered = new DeliveryMessage.NotDelivered(reminderMessage, "a reason");

            undeliveredProcessManager.Tell(notDelivered);
            ExpectMsg<DeliveryMessage.Rescheduled>();
            undeliveredProcessManager.Tell(new DeliveryMessage.Delivered(reminderMessage.ReminderId, now));
            ExpectNoMsg();
            undeliveredProcessManager.Tell(new QueryMessage.HowManyUndeliveredRemindersDoYouHave());
            ExpectMsg<QueryMessage.HowManyUndeliveredRemindersDoYouHaveResponse>(resp => resp.Count == 0);
        }
    }

    /*
    [TestFixture]
	public class When_cannot_deliver : 
		RoutableTestBase,
        IHandle<DeliveryMessage.Rescheduled>,
        IHandle<DeliveryMessage.Undeliverable>
	{

		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;
		private TimeSpan _durationToGiveup = 60.Minutes ();

		[TestFixtureSetUp]
		public void Initialize()
		{
			_processManager = new UndeliveredProcessManager (Bus);
            Subscribe<DeliveryMessage.Rescheduled>(this);
            Subscribe<DeliveryMessage.Undeliverable>(this);
			_originalReminder = MessageBuilders.BuildReminders (1, 3, SystemTime.UtcNow().Add(_durationToGiveup)).First ();
		}

		[Test]
		public void Run_Steps()
		{
			When_receive_an_undelivered_reminder (_originalReminder);
			while (!Undeliverable_message_received ()) {
				Should_emit_a_rescheduled_reminder_for_the_same_reminder (_originalReminder.ReminderId);
				When_receive_an_undelivered_reminder (_originalReminder);
			}

			//there should be 3 rescheduled reminders and one Undeliverable message
            Received.ContainsThisMany<DeliveryMessage.Rescheduled>(3);
            Received.ContainsOne<DeliveryMessage.Undeliverable>();

            var timeSinceOriginalWasDue = ((DeliveryMessage.Rescheduled)Received[Received.Count - 2]).DueAt - _originalReminder.DueAt;

			//we hit some rounding errors in the math using DateTime's, so lets make sure we are close enough
			Assert.That (timeSinceOriginalWasDue, Is.EqualTo (_durationToGiveup).Within (1).Seconds);
		}

		public void When_receive_an_undelivered_reminder(ReminderMessage.Schedule reminder)
		{
            var notDelivered = new DeliveryMessage.NotDelivered(reminder, "failed reason");
			_processManager.Handle (notDelivered);
		}

		public void Should_emit_a_rescheduled_reminder_for_the_same_reminder(Guid reminderId)
		{
			var msg = Received[Received.Count -1];
            Assert.IsInstanceOf<DeliveryMessage.Rescheduled>(msg);
            var received = (DeliveryMessage.Rescheduled)msg;
			Assert.AreEqual (received.ReminderId, reminderId);
		}

		private bool Undeliverable_message_received()
		{
			if (Received.Count == 0)
				return false;
            return Received[Received.Count - 1] is DeliveryMessage.Undeliverable;
		}

        public void Handle(DeliveryMessage.Rescheduled msg)
		{
			Received.Add (msg);
		}

        public void Handle(DeliveryMessage.Undeliverable msg)
		{
			Received.Add (msg);
		}
	}*/
}

