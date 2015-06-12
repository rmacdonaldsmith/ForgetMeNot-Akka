using System;
using System.Collections.Generic;
using ForgetMeNot.Common;
using ForgetMeNot.Router;

namespace ForgetMeNot.Messages
{
	public static class ReminderMessage
	{
		public enum TransportEnum
		{
			http,
			rabbitmq
		}

		public enum ContentEncodingEnum
		{
			utf8
		}

		public interface IReminder : IMessage
		{
			Guid ReminderId { get; set; }
		}

		public interface IDeliverable : IReminder
		{
			string DeliveryUrl { get; set; }
			string ContentType { get; set; }
			byte[] Payload { get; set; }
		}

		public interface ISchedulable : IReminder
		{
			DateTime DueAt { get; set; }
		}

		public class Schedule : ISchedulable, IDeliverable, IReminder
		{
			public Guid ReminderId { get; set; }
			public DateTime DueAt { get; set; }
			public DateTime? GiveupAfter { get; set; }
			public int MaxRetries { get; set; }
			public string DeliveryUrl { get; set; }
			public string ContentType { get; set; }
			public ContentEncodingEnum ContentEncoding { get; set; }
			public TransportEnum Transport { get; set; }
			public byte[] Payload { get; set; }
			public string Tag { get; set; }

			public Schedule ()
			{
				//default constructor
			}

			public Schedule (DateTime dueAt, string deliveryUrl, string contentType, ContentEncodingEnum encoding, TransportEnum transport, byte[] payload, int maxAttempts, DateTime? giveupAfter = null, string tag = null)
			{
				DueAt = dueAt;
				MaxRetries = maxAttempts;
				GiveupAfter = giveupAfter;
				DeliveryUrl = deliveryUrl;
				ContentType = contentType;
				ContentEncoding = encoding;
				Transport = transport;
				Payload = payload;
				Tag = tag;
			}

			public Schedule (Guid reminderId, DateTime dueAt, string deliveryUrl, string contentType, ContentEncodingEnum encoding, TransportEnum transport, byte[] payload, int maxAttempts, DateTime? giveupAfter = null, string tag = null)
				: this(dueAt, deliveryUrl, contentType, encoding, transport, payload, maxAttempts, giveupAfter, tag)
			{
				ReminderId = reminderId;
			}
		}

		public class Due : IReminder
		{
			public Guid ReminderId { get; set; }
			public ReminderMessage.Schedule Reminder { get; set; }

			public Due (ReminderMessage.Schedule due)
			{
				Reminder = due;
				ReminderId = due.ReminderId;
			}
		}

		public class Cancel : IReminder
		{
			public Guid ReminderId { get; set; }

			public Cancel (Guid reminderId)
			{
				ReminderId = reminderId;
			}
		}

		public class Delivered : IReminder
		{
			public Guid ReminderId { get; set; }
			public DateTime SentStamp { get; private set; }

			public Delivered (Guid reminderId, DateTime sentStamp)
			{
				Ensure.NotEmptyGuid(reminderId, "reminderId");

				ReminderId = reminderId;
				SentStamp = sentStamp;
			}
		}

		public class Undeliverable : IReminder
		{
			public Guid ReminderId { get; set; }
			public ReminderMessage.Schedule Reminder { get; set; }
			public string Reason { get; set; }

			public Undeliverable (ReminderMessage.Schedule reminder, string reason)
			{
				Reminder = reminder;
				Reason = reason;
				ReminderId = reminder.ReminderId;
			}
		}

		public class Rescheduled : ISchedulable
		{
			public ReminderMessage.Schedule Reminder { get; set; }
			public DateTime DueAt { get; set; }
			public Guid ReminderId { get; set; }

			public Rescheduled (ReminderMessage.Schedule original, DateTime rescheduledFor)
			{
				Reminder = original;
				DueAt = rescheduledFor;
				ReminderId = Reminder.ReminderId;
			}
		}

		public class SentToDeadLetter : Delivered
		{
			public SentToDeadLetter (Guid reminderId, DateTime sentStamp) 
				: base (reminderId, sentStamp)
			{
				//empty	
			}
		}

		public class Undelivered : IReminder
		{
			public Guid ReminderId { get; set; }
			public ReminderMessage.Schedule Reminder { get; set; }
			public string Reason { get; set; }

			public Undelivered (ReminderMessage.Schedule reminder, string reason)
			{
				ReminderId = reminder.ReminderId;
				Reminder = reminder;
				Reason = reason;
			}
		}

		public class EqualityComparer<T> : IEqualityComparer<T> where T : IMessage
		{
			private readonly Func<T, int> _getHashCode;
			private readonly Func<T, T, bool> _equals;

			public EqualityComparer (Func<T, int> getHashCodeDelegate, Func<T, T, bool> equalsDelegate)
			{
				Ensure.NotNull(getHashCodeDelegate, "getHashCodeDelegate");
				Ensure.NotNull(equalsDelegate, "equalsDelegate");

				_getHashCode = getHashCodeDelegate;
				_equals = equalsDelegate;
			}

			public bool Equals (T x, T y)
			{
				return _equals (x, y);
			}

			public int GetHashCode (T obj)
			{
				return _getHashCode (obj);
			}
		}
	}
}

