using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Core.Tests.Helpers;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using ForgetMeNot.Test.Common;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.Startup
{
	[TestFixture]
	public class OnSuccessfulStartUp : 
		RoutableTestBase, 
		IConsume<SystemMessage.InitializationCompleted>, 
		IConsume<ReminderMessage.Cancel>, 
		IConsume<ReminderMessage.Schedule>
	{
		[TestFixtureSetUp]
		public void When_the_system_starts()
		{
			//_startupManager.Handle (new SystemMessage.Start ());
		}

		[Test]
		public void Then_should_send_InitializationCompleted_event ()
		{
			_startupManager.Handle (new SystemMessage.Start ());

			Assert.IsTrue (Received.ContainsOne<SystemMessage.InitializationCompleted> ());
			Assert.AreEqual (5, _cancellationCount);
			Assert.AreEqual (5, _scheduleCount);
		}



		private IObservable<ReminderMessage.IReminder> BuildCancelEventStream(int count)
		{
			return Observable
				.Range (0, count)
				.Select (x => new ReminderMessage.Cancel (Guid.NewGuid()))
				.Cast<ReminderMessage.IReminder> ();
		}

		private IObservable<ReminderMessage.IReminder> BuildCurrentReminderStream(int count)
		{
			return Observable
				.Range (0, count)
				.Select (x => new ReminderMessage.Schedule ())
				.Cast<ReminderMessage.IReminder> ();
		}

		public void Handle (SystemMessage.InitializationCompleted msg)
		{
			Received.Add (msg);
		}

		public void Handle (ReminderMessage.Cancel msg)
		{
			_cancellationCount++;
		}

		public void Handle (ReminderMessage.Schedule msg)
		{
			_scheduleCount++;
		}

		private SystemStartManager _startupManager;
		private int _cancellationCount = 0;
		private int _scheduleCount = 0;

		public OnSuccessfulStartUp ()
		{
			var cancellationReplayer = new FakeReplayer ((from) => this.BuildCancelEventStream(5));
			var currentRemindersReplayer = new FakeReplayer ((from) => this.BuildCurrentReminderStream(5));
			_startupManager = new SystemStartManager (Bus, new List<IReplayEvents>{cancellationReplayer, currentRemindersReplayer});
			Subscribe ((IConsume<SystemMessage.InitializationCompleted>)this);
			Subscribe ((IConsume<ReminderMessage.Cancel>)this);
			Subscribe ((IConsume<ReminderMessage.Schedule>)this);
		}
	}
}

