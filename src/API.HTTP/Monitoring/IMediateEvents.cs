using System;

namespace ForgetMeNot.API.HTTP.Monitoring
{
	public interface IMediateEvents
	{
		IObservable<MonitorEvent> GetStream { get; }
	}
}

