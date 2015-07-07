using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;

namespace ForgetMeNot.Core.Startup
{
	public class SystemStartManager : TypedActor, IHandle<SystemMessage.BeginInitialization>
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof(SystemStartManager));
		private readonly List<IReplayEvents> _replayers;
	    private readonly IActorRef _bus;

		public SystemStartManager (IEnumerable<IReplayEvents> replayers)
		{
			Ensure.NotNull (replayers, "replayers");

			_replayers = new List<IReplayEvents> (replayers);
		}

		public void Handle (SystemMessage.BeginInitialization init)
		{
			// Merge the observable sequences from all the replayers in to one stream
			// Play that stream over the bus to initialize components
			// When all observable sequences have completed, then we send a message indicating that init is completed => start normal operation
			// If any of the child observables error, then the merged observable will error => we will not publish the init completed message => the system will not start
			Observable
				.Merge (_replayers.Select (r => r.Replay<ReminderMessage.IReminder> (SystemTime.Now ())))
				.Subscribe (
				Observer.Create<ReminderMessage.IReminder> (
					message => _bus.Tell(message), 								// OnNext
					error =>                                                        // OnError
					    {
					        var msg = "There was an exception while replaying data in to the system";
					        Logger.Error(msg, error);
					        throw new SystemInitializationException(msg, error);
					    },
					    () =>                                                       // OnCompleted
					        _bus.Tell(new SystemMessage.InitializationCompleted())));
		}
	}
}

