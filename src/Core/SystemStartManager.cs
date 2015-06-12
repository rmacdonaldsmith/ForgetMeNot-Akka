using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core
{
	public class SystemStartManager : ReceiveActor
	{
		private readonly List<ActorRef> _replayers;

		public SystemStartManager (IEnumerable<IActorRef> replayers)
		{
			Ensure.NotNull (replayers, "replayers");

			_replayers = new List<IReplayEvents> (replayers);
		}

		public void Receive (SystemMessage.Start start)
		{
			// Merge the observable sequences from all the replayers in to one stream
			// Play that stream over the bus to initialize components
			// When all observable sequences have completed, then we send a message indicating that init is completed => start normal operation
			// If any of the child observables error, then the merged observable will error => we will not publish the init completed message => the system will not start
			Observable
				.Merge (_replayers.Select (r => r.Replay<IMessage> (SystemTime.Now ())))
				.Subscribe (
				Observer.Create<IMessage> (
					message => _bus.Send (message), 								// OnNext
					error =>                                                        // OnError
					    {
					        var msg = "There was an exception while replaying data in to the system";
					        Logger.Error(msg, error);
					        throw new SystemInitializationException(msg, error);
					    },
					    () =>                                                       // OnCompleted
					    {
					        _bus.Send(new SystemMessage.InitializationCompleted());
					    }	
				));
		}
	}
}

