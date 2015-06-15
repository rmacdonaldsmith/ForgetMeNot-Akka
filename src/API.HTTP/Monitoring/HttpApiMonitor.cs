using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ForgetMeNot.API.HTTP.Models;
using ForgetMeNot.Common;

namespace ForgetMeNot.API.HTTP.Monitoring
{
	public class HttpApiMonitor
	{
		private readonly int _windowSize;
		private readonly int _windowSlide;
		private readonly IMediateEvents _eventMediator;
		private readonly Subject<MonitorEvent> _subject;
		private IObservable<MonitorEvent> _stream;
		private readonly Dictionary<string, MonitorGroup> _monitors = new Dictionary<string, MonitorGroup>();

		public HttpApiMonitor (IMediateEvents mediator, int windowSize, int windowSlide)
		{
			Ensure.NotNull (mediator, "mediator");

			_eventMediator = mediator;
			_windowSize = windowSize;
			_windowSlide = windowSlide;

			CreateStream (_eventMediator);
		}

		private void CreateStream(IMediateEvents mediator)
		{
			_stream = mediator.GetStream;
			_stream
				.Window(_windowSize)
				.Switch()
				.GroupBy(w => w.Topic)
				.Subscribe(groupedByTopic => groupedByTopic
					.GroupBy(g => g.Key)
					.Subscribe(groupedByKey => 
						groupedByKey
						.Aggregate(new WindowState (), WindowState.Calculate)
						.Subscribe(windowState => UpdateMonitors(windowState))
					)
				);
		}

		private void UpdateMonitors(WindowState window)
		{
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "First" + window.ItemName,
				Value = window.First.Value.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "Last" + window.ItemName,
				Value = window.Last.Value.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value, 
				Key = "Min" + window.ItemName,
				Value = window.Min.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "Max" + window.ItemName,
				Value = window.Max.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "Mean" + window.ItemName,
				Value = window.Mean.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "WindowCount",
				Value = window.WindowCount.ToString ()
			});
			UpdateMonitor (new MonitorItem {
				Topic = window.ComponentName,
				TimeStamp = window.Last.Value,
				Key = "WindowDurationMs",
				Value = window.WindowDuration.TotalMilliseconds.ToString ()
			});
		}

		private void UpdateMonitor(MonitorItem item)
		{
			if (!_monitors.ContainsKey (item.Topic))
				_monitors.Add (item.Topic, MonitorGroup.Create (item));
			else
				_monitors [item.Topic].Upsert (item);
		}

		public List<MonitorGroup> GetMonitors()
		{
			//todo: lock this region and in the UpdateMonitors method.
			return new List<MonitorGroup>(_monitors.Values);
		}

		private class WindowState
		{
			public string ComponentName { get; set; }
			public string ItemName { get; set; }
			public DateTime? First { get; set; }
			public DateTime? Last { get; set; }
			public TimeSpan WindowDuration { get; set; }
			public int WindowCount { get; set; }
			public int Min { get; set; }
			public int Max { get; set; }
			public int Sum { get; set; }
			public int Mean { get; set; }

			public static WindowState Calculate(WindowState item, MonitorEvent evnt) {

				item.ComponentName = evnt.Topic;
				item.ItemName = evnt.Key;
				item.First = item.First ?? evnt.TimeStamp;
				item.Last = evnt.TimeStamp;
				item.WindowDuration = (item.First.HasValue && item.Last.HasValue) ? item.Last.Value.Subtract (item.First.Value) : TimeSpan.Zero;
				item.WindowCount = item.WindowCount + 1;
				item.Min = evnt.Value < item.Min || item.Min == 0 ? (int)evnt.Value : item.Min;
				item.Max = evnt.Value > item.Max ? (int)evnt.Value : item.Max;
				item.Sum = item.Sum + (int)evnt.Value;
				item.Mean = item.WindowCount > 0 ? item.Sum / item.WindowCount : 0;
				return item;
			}

			public override string ToString ()
			{
				return string.Format ("[WindowState: ComponentName={0}, ItemName={1}, First={2}, Last={3}, WindowDuration={4}, WindowCount={5}, Min={6}, Max={7}, Sum={8}, Mean={9}]", ComponentName, ItemName, First, Last, WindowDuration, WindowCount, Min, Max, Sum, Mean);
			}
		}
	}
}

