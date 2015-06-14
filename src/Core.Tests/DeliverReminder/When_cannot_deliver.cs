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
	[TestFixture]
	public class When_cannot_deliver : 
		RoutableTestBase, 
		IConsume<ReminderMessage.Rescheduled>,
		IConsume<ReminderMessage.Undeliverable>
	{

		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;
		private TimeSpan _durationToGiveup = 60.Minutes (); //TimeSpan.FromMinutes(60);

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
			while (!Undeliverable_message_received ()) {
				Should_emit_a_rescheduled_reminder_for_the_same_reminder (_originalReminder.ReminderId);
				When_receive_an_undelivered_reminder (_originalReminder);
			}

			//there should be 3 rescheduled reminders and one Undeliverable message
			Received.ContainsThisMany<ReminderMessage.Rescheduled>(3);
			Received.ContainsOne<ReminderMessage.Undeliverable> ();

			var timeSinceOriginalWasDue = ((ReminderMessage.Rescheduled)Received [Received.Count - 2]).DueAt - _originalReminder.DueAt;

			//we hit some rounding errors in the math using DateTime's, so lets make sure we are close enough
			Assert.That (timeSinceOriginalWasDue, Is.EqualTo (_durationToGiveup).Within (1).Seconds);
		}

		public void When_receive_an_undelivered_reminder(ReminderMessage.Schedule reminder)
		{
			var undelivered = new ReminderMessage.Undelivered (reminder, "failed reason");
			_processManager.Handle (undelivered);
		}

		public void Should_emit_a_rescheduled_reminder_for_the_same_reminder(Guid reminderId)
		{
			var msg = Received[Received.Count -1];
			Assert.IsInstanceOf<ReminderMessage.Rescheduled> (msg);
			var received = (ReminderMessage.Rescheduled)msg;
			Assert.AreEqual (received.ReminderId, reminderId);
		}

		private bool Undeliverable_message_received()
		{
			if (Received.Count == 0)
				return false;
			return Received [Received.Count - 1] is ReminderMessage.Undeliverable;
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

