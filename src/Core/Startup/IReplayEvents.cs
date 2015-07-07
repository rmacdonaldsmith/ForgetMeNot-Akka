using System;

namespace ForgetMeNot.Core.Startup
{
	public interface IReplayEvents
	{
		IObservable<T> Replay<T>(DateTime from);
	}
}

