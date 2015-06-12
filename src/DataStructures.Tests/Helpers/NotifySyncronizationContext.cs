using System;
using System.Threading;

namespace ReminderService.DataStructures.Tests.Helpers
{
	public class NotifySyncronizationContext : SynchronizationContext
	{
		public NotifySyncronizationContext ()
		{
			//empty
		}

		public event EventHandler NotifyCompleted;

		public override void Post(SendOrPostCallback d, object state)
		{
			d.Invoke(state);
			NotifyCompleted(this, System.EventArgs.Empty);
		}
	}
}

