using System;
using ForgetMeNot.Common;

namespace ForgetMeNot.Messages
{
	public static class ReminderExtensions
	{
		public static ReminderMessage.Due AsDue(this ReminderMessage.ISchedulable source)
		{
			if(source is ReminderMessage.Schedule)
				return new ReminderMessage.Due ((ReminderMessage.Schedule)source);

			if (source is ReminderMessage.Rescheduled)
				return new ReminderMessage.Due (((ReminderMessage.Rescheduled)source).Reminder);

			throw new InvalidOperationException (string.Format("Can not create a Due message. Can not convert from [{0}]", source.GetType().FullName));
		}

		public static ReminderMessage.Delivered AsSent(this ReminderMessage.Due source)
		{
			return AsSent (source, SystemTime.Now());
		}

		public static ReminderMessage.Delivered AsSent(this ReminderMessage.Due source, DateTime sentStamp)
		{
			return new ReminderMessage.Delivered (source.ReminderId, sentStamp);
		}

		public static bool DoNotAttemptRedelivery(this ReminderMessage.Undelivered undelivered)
		{
			return !undelivered.Reminder.GiveupAfter.HasValue;
		}
	}
}

