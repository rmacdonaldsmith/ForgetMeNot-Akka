using System.Linq;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Core.Tests.Helpers;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using ForgetMeNot.Test.Common;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.DeliverReminder
{
	[TestFixture]
	public class When_redelivery_should_not_be_attempted : 
		RoutableTestBase, 
		IConsume<ReminderMessage.Schedule>,
		IConsume<ReminderMessage.Undeliverable>
	{
		private UndeliveredProcessManager _processManager;
		private ReminderMessage.Schedule _originalReminder;

		[TestFixtureSetUp]
		public void Initialize()
		{
			_processManager = new UndeliveredProcessManager (Bus);
			Subscribe<ReminderMessage.Schedule>(this);
			Subscribe<ReminderMessage.Undeliverable> (this);
			When_receive_an_undelivered_reminder ();
		}

		public void When_receive_an_undelivered_reminder()
		{
			_originalReminder = MessageBuilders.BuildReminders (1).First ();
			var undelivered = new ReminderMessage.Undelivered (_originalReminder, "failed reason");
			_processManager.Handle (undelivered);
		}

		[Test]
		public void Should_receive_an_Undeliverable_message()
		{
			Received.ContainsOne<ReminderMessage.Undeliverable>();
			var received = (ReminderMessage.Undeliverable)Received.First ();
			Assert.AreEqual (received.ReminderId, _originalReminder.ReminderId);
		}

		[Test]
		public void Should_not_attempt_to_change_the_DueAt_time()
		{
			Received.ContainsOne<ReminderMessage.Undeliverable>();
			var received = (ReminderMessage.Undeliverable)Received.First ();
			Assert.AreEqual (received.Reminder.DueAt, _originalReminder.DueAt);
		}

		public void Handle (ReminderMessage.Schedule msg)
		{
			Received.Add (msg);
		}

		public void Handle (ReminderMessage.Undeliverable msg)
		{
			Received.Add (msg);
		}
	}
}

