using System;
using System.Linq;
using ForgetMeNot.Common;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Core.Tests.Helpers;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using ForgetMeNot.Test.Common;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.DeliverReminder
{
	[TestFixture ()]
	public class When_attempting_redelivery : 
		RoutableTestBase, 
		IConsume<ReminderMessage.Rescheduled>,
		IConsume<ReminderMessage.Undeliverable>
	{

		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;
		private TimeSpan _durationToGiveup = 60.Minutes();

		[TestFixtureSetUp]
		public void Initialize()
		{
			_processManager = new UndeliveredProcessManager (Bus);
			Subscribe<ReminderMessage.Rescheduled>(this);
			Subscribe<ReminderMessage.Undeliverable> (this);
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
			var undelivered = new ReminderMessage.Undelivered (reminder, "failed reason");
			_processManager.Handle (undelivered);
		}

		private void When_receive_a_Delivered_message(Guid reminderId)
		{
			var delivered = new ReminderMessage.Delivered (reminderId, SystemTime.UtcNow());
			_processManager.Handle (delivered);
		}

		private void Should_emit_a_rescheduled_reminder_for_the_same_reminder(Guid reminderId)
		{
			var msg = Received[Received.Count -1];
			Assert.IsInstanceOf<ReminderMessage.Rescheduled> (msg);
			var received = (ReminderMessage.Rescheduled)msg;
			Assert.AreEqual (received.ReminderId, reminderId);
		}

		public void Handle (ReminderMessage.Rescheduled msg)
		{
			Received.Add (msg);
		}

		public void Handle (ReminderMessage.Undeliverable msg)
		{
			Received.Add (msg);
		}
	}
}

