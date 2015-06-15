using System;
using System.Reactive.Subjects;

namespace ForgetMeNot.API.HTTP.Monitoring
{
//	public class ObservableMonitorEvent : IObservable<MonitorEvent>
//	{
//		private readonly Subject<MonitorEvent> _subject = new Subject<MonitorEvent> ();
//
//		public ObservableMonitorEvent ()
//		{
//		}
//
//		public IDisposable Subscribe (IObserver<MonitorEvent> observer)
//		{
//			return _subject.Subscribe (observer);
//		}
//
//		public void PushEvent(MonitorEvent evnt)
//		{
//			_subject.OnNext (evnt);
//		}
//	}

	public class ObservableMonitor<T> : IObservable<T>
	{
		private readonly Subject<T> _subject = new Subject<T> ();

		public ObservableMonitor ()
		{
		}

		public IDisposable Subscribe (IObserver<T> observer)
		{
			return _subject.Subscribe (observer);
		}

		public void PushEvent(T evnt)
		{
			_subject.OnNext (evnt);
		}
	}
}

