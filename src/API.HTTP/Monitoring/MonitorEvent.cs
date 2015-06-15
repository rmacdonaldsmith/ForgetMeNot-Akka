using System;

namespace ForgetMeNot.API.HTTP.Monitoring
{
	public class MonitorEvent
	{
		public string Topic { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Key { get; set; }
		public long Value { get; set; }

		public MonitorEvent (string topic, DateTime timeStamp, string key, long value)
		{
			Topic = topic;
			TimeStamp = timeStamp;
			Key = key;
			Value = value;
		}

		public override string ToString ()
		{
			return string.Format ("[MonitorEvent: Topic={0}, TimeStamp={1}, Key={2}, Value={3}]", Topic, TimeStamp, Key, Value);
		}
	}
}

