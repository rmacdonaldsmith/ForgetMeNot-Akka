using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using ReminderService.DataStructures;
using ReminderService.Common;
using Timer = System.Timers.Timer;

namespace ReminderService.DataStructures
{
    public class TimeoutController
    {
		private readonly MinPriorityQueue<ScheduledReminder> _pq;
		//private readonly Timer _timer;
		private int _running = 0;
		private Action<IEnumerable<ScheduledReminder>> _timeoutCallback;
		private CancellationTokenSource _cancelationToken = new CancellationTokenSource();
		private TaskScheduler _scheduler;

		/// <summary>
		/// You can override the default scheduler with this constructor. Intended for unit testing.
		/// </summary>
		/// <param name="timeoutCallback">This funtion will be called when reminders are due.</param>
		/// <param name="scheduler">Scheduler.</param>
		public TimeoutController(Action<IEnumerable<ScheduledReminder>> timeoutCallback, TaskScheduler scheduler)
			: this(timeoutCallback)
		{
			_scheduler = scheduler;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReminderService.DataStructures.TimeoutController"/> class.
		/// </summary>
		/// <param name="timeoutCallback">This funtion will be called when reminders are due.</param>
		public TimeoutController(Action<IEnumerable<ScheduledReminder>> timeoutCallback)
        {
			_pq = new MinPriorityQueue<ScheduledReminder>(100);
			_scheduler = TaskScheduler.Default;
//            _timer = new Timer();
//            _timer.Elapsed += TimerOnElapsed;
			_timeoutCallback = timeoutCallback;
        }

        public void Start()
        {
			Interlocked.Increment (ref _running);
			Task.Run (() => GetNextTimeout ());
        }

		private void GetNextTimeout()
        {
			if (_running > 0 && !_pq.IsEmpty)
            {
				var nextTimeoutAt = _pq.Min ().TimeOutAt;
				var timeToNext = nextTimeoutAt.Subtract(SystemTime.Now()).Milliseconds;
				Console.WriteLine ("GetNextTimeout, timeToNext: " + timeToNext);
				try
				{
				Task
					.Delay (timeToNext, _cancelationToken.Token)
					.ContinueWith ((task) => {
							Console.WriteLine(string.Format("Reminder due at {0:H:mm:ss fff}", nextTimeoutAt));
							var remindersDue = _pq.GetRemindersAtTime(nextTimeoutAt);
							_timeoutCallback(remindersDue); //do we want to run this async?
							GetNextTimeout();
						});
				}
				catch (TaskCanceledException tce) {
					// task has been canceled because a new incoming timeout will expire before the current timeout
					Console.WriteLine (string.Format("Task cancelled for time {0:H:mm:ss fff}", nextTimeoutAt));
					GetNextTimeout ();
				}
				catch(Exception ee){
					//something is wrong -> stop everything, we're in trouble
					Stop ();
					//log
					throw;
				}
            }
        }

        public void Stop()
        {
			Interlocked.Decrement (ref _running);
			_cancelationToken.Cancel ();
        }

		public bool IsRunning
		{
			get { return _running != 0; }
		}

		public void Add(ScheduledReminder reminder)
		{
			//need to check if this incoming reminder is going to timeout sooner than the current reminder
			//on the top of the queue.
			//if so, we need to add to the queue (so that it is resorted) and then get the new current min
			if (!_pq.IsEmpty && _pq.Min ().TimeOutAt.Subtract (reminder.TimeOutAt) > TimeSpan.Zero) {
				Console.WriteLine (
					string.Format ("Inserting reminder {0} {1:H:mm:ss fff}, cancelling previous", 
						reminder.ReminderId, reminder.TimeOutAt));
				_cancelationToken.Cancel ();
				_pq.Insert (reminder);
			}
			else
				Console.WriteLine (
					string.Format("Inserting reminder {0} {1:H:mm:ss fff}", reminder.ReminderId, reminder.TimeOutAt));
				_pq.Insert (reminder);

			GetNextTimeout ();
		}
    }
}
