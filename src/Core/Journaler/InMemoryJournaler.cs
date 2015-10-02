using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ForgetMeNot.Core.Journaler
{
	public class InMemoryJournaler : IJournalEvents
	{
		private readonly ConcurrentQueue<object> _messages;

		public InMemoryJournaler ()
		{
			_messages = new ConcurrentQueue<object> ();
		}
			
		public void Write (object message)
		{
			_messages.Enqueue (message);
		}

		public IList<object> JournaledMessages {
			get { return _messages.ToList(); }
		}
	}
}

