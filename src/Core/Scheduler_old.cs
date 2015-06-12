using System;
using System.Threading;
using ForgetMeNot.Common;
using ForgetMeNot.DataStructures;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using log4net;

namespace ForgetMeNot.Core.ScheduleReminder
{
	public class Scheduler : 
		IConsume<Envelopes.Journaled<ReminderMessage.Schedule>>, 
		IConsume<ReminderMessage.Rescheduled>,
		IConsume<SystemMessage.Start>, 
		IConsume<SystemMessage.ShutDown>,
	IHandleQueries<QueryResponse.GetQueueStats, QueryResponse.QueueStats>,
		IDisposable
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof(Scheduler));
		private readonly object _locker = new object ();
		private readonly ISendMessages _bus;
		private readonly ITimer _timer;
		private readonly MinPriorityQueue<ReminderMessage.ISchedulable> _pq;
		private int _running = 0;
	    private readonly int _timerIntervalMs;

		public Scheduler (ISendMessages bus, ITimer timer, int seedSize, int timerIntervalMs)
		{
			Ensure.NotNull (bus, "bus");
			Ensure.NotNull (timer, "timer");
			Ensure.Nonnegative (seedSize, "seedSize");
            Ensure.Nonnegative(timerIntervalMs, "timerIntervalMs");

			_bus = bus;
			_timer = timer;
		    _timerIntervalMs = timerIntervalMs;
		    _pq = new MinPriorityQueue<ReminderMessage.ISchedulable> (seedSize, (a, b) => a.DueAt > b.DueAt);
		}

		public QueryResponse.QueueStats Handle (QueryResponse.GetQueueStats request)
		{
			lock (_locker) {
				return new QueryResponse.QueueStats{ QueueSize = _pq.Size };
			}
		}
			
		public void Handle (SystemMessage.Start startMessage)
		{
			Start ();
		}

		public void Handle (SystemMessage.ShutDown stopMessage)
		{
			Stop ();
		}

		public void Handle (Envelopes.Journaled<ReminderMessage.Schedule> journaled)
		{
			Logger.DebugFormat ("Scheduling reminder [{0}]", journaled.Message.ReminderId);
			lock (_locker) {
				_pq.Insert (journaled.Message);
				SetTimeout ();
			}
		}

		public void Handle(ReminderMessage.Rescheduled rescheduled)
		{
			Logger.DebugFormat ("Re-scheduling reminder [{0}]", rescheduled.ReminderId);
			lock (_locker) {
				_pq.Insert (rescheduled);
				SetTimeout ();
			}
		}

		private void OnTimerFired()
		{
			//get all the items from the pq that are due
			lock (_locker) {
				while (!_pq.IsEmpty && _pq.Min ().DueAt <= SystemTime.UtcNow()) {
					var due = _pq.RemoveMin ().AsDue ();
					Logger.DebugFormat ("Timer fired. Dequeing reminder {0}", due.ReminderId);
					_bus.Send (due);
				}
				Logger.InfoFormat (new {queuesize = _pq.Size, logname = "metrics"}, "Queue size");
				SetTimeout ();
			}
		}

		private void SetTimeout()
		{
			if (_running > 0 && !_pq.IsEmpty)
			{
				//var nextTimeoutAt = _pq.Min ().DueAt;
				//var timeToNext = Convert.ToInt32 (nextTimeoutAt.Subtract (SystemTime.UtcNow ()).TotalMilliseconds); //Int32.MaxValue in milliseconds is about 68 years! Hopefully nobody is going to schedule something that far in the future
				//Logger.DebugFormat ("Setting next timeout for {0}ms", timeToNext);
				_timer.FiresIn (_timerIntervalMs, OnTimerFired);
			}
		}

		public void Start()
		{
			SetTimeout ();
			Interlocked.CompareExchange (ref _running, 1, 0);
		}

		public void Stop()
		{
			Interlocked.Decrement (ref _running);
		}

		public bool IsRunning
		{
			get { return _running != 0; }
		}

		public void Dispose ()
		{
			_timer.Dispose ();
		}
	}
}

