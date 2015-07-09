using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ForgetMeNot.Common;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Core.Tests.Helpers;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using ForgetMeNot.Test.Common;
using NUnit.Framework;

namespace ForgetMeNot.Core.Tests.Startup
{
	[TestFixture]
	public class WhenAReplayFails : RoutableTestBase, IConsume<SystemMessage.InitializationCompleted>
	{
		[TestFixtureSetUp]
		public void When_the_system_starts()
		{
		    try
		    {
		        _startupManager.Handle(new SystemMessage.Start());
		    }
		    catch (SystemInitializationException sie)
		    {
                //swallow, we are expecting to receive this exception.
		    }
		    catch (Exception e)
		    {
		        Assert.Fail("unexpected exception received.");
		    }
		}

		[Test]
		public void Then_should_not_send_the_InitializationCompleted_event ()
		{
			Received.DoesNotContain <SystemMessage.InitializationCompleted>();
		}

		public void Handle (SystemMessage.InitializationCompleted msg)
		{
			Received.Add (msg);
		}

		private SystemStartManager _startupManager;

		public WhenAReplayFails ()
		{
			var cancellationReplayer = new FakeReplayer ((from) => 
				Observable
					.Range (0, 5)
					.Select (x => new ReminderMessage.Cancel (Guid.NewGuid ()))
					.Cast<IMessage> ());

			var currentRemindersReplayer = new FakeReplayer ((from) =>
				Observable.Throw<ReminderMessage.Schedule>(new Exception("Something went wrong in the database")));

			_startupManager = new SystemStartManager (Bus, new List<IReplayEvents>{cancellationReplayer, currentRemindersReplayer});
			Subscribe (this);
		}
	}
}

