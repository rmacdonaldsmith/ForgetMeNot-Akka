using System;
using FluentAssertions;
using ForgetMeNot.Common;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Messages;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.DeliverReminder
{
	[TestFixture ()]
	public class When_attempting_redelivery : 
		RoutableTestBase,
        IConsume<DeliveryMessage.Rescheduled>,
        IConsume<DeliveryMessage.Undeliverable>
	{

		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;
		private TimeSpan _durationToGiveup = 60.Minutes();

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
			Should_emit_a_rescheduled_reminder_for_the_same_reminder (_originalReminder.ReminderId);
			When_receive_an_undelivered_reminder (_originalReminder);
			Should_emit_a_rescheduled_reminder_for_the_same_reminder (_originalReminder.ReminderId);
			When_receive_a_Delivered_message (_originalReminder.ReminderId);

			//there should be 2 Rescheduled reminders and zero Undeliverable
			Received.ContainsThisMany<ReminderMessage.Rescheduled>(2);
			Received.DoesNotContain<ReminderMessage.Undeliverable> ();
		}

		private void When_receive_an_undelivered_reminder(ReminderMessage.Schedule reminder)
		{
            var notDelivered = new DeliveryMessage.NotDelivered(reminder, "failed reason");
			_processManager.Handle (notDelivered);
		}

		private void When_receive_a_Delivered_message(Guid reminderId)
		{
            var delivered = new DeliveryMessage.Delivered(reminderId, SystemTime.UtcNow());
			_processManager.Handle (delivered);
		}

		private void Should_emit_a_rescheduled_reminder_for_the_same_reminder(Guid reminderId)
		{
			var msg = Received[Received.Count -1];
            Assert.IsInstanceOf<DeliveryMessage.Rescheduled>(msg);
            var received = (DeliveryMessage.Rescheduled)msg;
			Assert.AreEqual (received.ReminderId, reminderId);
		}

        public void Handle(DeliveryMessage.Rescheduled msg)
		{
			Received.Add (msg);
		}

        public void Handle(DeliveryMessage.Undeliverable msg)
		{
			Received.Add (msg);
		}
	}
}

