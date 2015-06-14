using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.DeliverReminder
{
	[TestFixture]
	public class When_redelivery_should_not_be_attempted : 
		RoutableTestBase, 
		IConsume<ReminderMessage.Schedule>,
        IConsume<DeliveryMessage.Undeliverable>
	{
		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;

		[TestFixtureSetUp]
		public void Initialize()
		{
			_processManager = new UndeliveredProcessManager (Bus);
			Subscribe<ReminderMessage.Schedule>(this);
            Subscribe<DeliveryMessage.Undeliverable>(this);
			When_receive_an_undelivered_reminder ();
		}

		public void When_receive_an_undelivered_reminder()
		{
			_originalReminder = MessageBuilders.BuildReminders (1).First ();
            var notDelivered = new DeliveryMessage.NotDelivered(_originalReminder, "failed reason");
			_processManager.Handle (notDelivered);
		}

		[Test]
		public void Should_receive_an_Undeliverable_message()
		{
            Received.ContainsOne<DeliveryMessage.Undeliverable>();
            var received = (DeliveryMessage.Undeliverable)Received.First();
			Assert.AreEqual (received.ReminderId, _originalReminder.ReminderId);
		}

		[Test]
		public void Should_not_attempt_to_change_the_DueAt_time()
		{
            Received.ContainsOne<DeliveryMessage.Undeliverable>();
            var received = (DeliveryMessage.Undeliverable)Received.First();
			Assert.AreEqual (received.Reminder.DueAt, _originalReminder.DueAt);
		}

		public void Handle (ReminderMessage.Schedule msg)
		{
			Received.Add (msg);
		}

        public void Handle(DeliveryMessage.Undeliverable msg)
		{
			Received.Add (msg);
		}
	}
}

