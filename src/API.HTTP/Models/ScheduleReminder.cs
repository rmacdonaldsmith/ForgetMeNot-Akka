namespace ForgetMeNot.API.HTTP.Models
{
	public class ScheduleReminder
	{
		public string DueAt { get; set; }
		public string GiveupAfter { get; set; }
		public int MaxRetries { get; set; }
		public string DeliveryUrl { get; set; }
		public string ContentType { get; set; }
		public string Encoding { get; set; }
		public string Transport { get; set;}
		public byte[] Payload { get; set; }
		public string Tag { get; set; }

		public ScheduleReminder ()
		{
			//empty
		}

		public ScheduleReminder (string dueAt, string deliveryUrl, string contentType, string encoding, string transport, byte[] payload, int maxRetries, string giveupAfter, string tag)
		{
			DueAt = dueAt;
			DeliveryUrl = deliveryUrl;
			ContentType = contentType;
			Encoding = encoding;
			Transport = transport;
			Payload = payload;
			MaxRetries = maxRetries;
			GiveupAfter = giveupAfter;
			Tag = tag;
		}
	}
}
