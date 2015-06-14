using System;
using ForgetMeNot.Common;

namespace ForgetMeNot.Messages
{
	public static class MessageExtensions
	{
		public static ReminderMessage.Due AsDue(this ReminderMessage.ISchedulable source)
		{
			if(source is ReminderMessage.Schedule)
				return new ReminderMessage.Due ((ReminderMessage.Schedule)source);

			if (source is DeliveryMessage.Rescheduled)
				return new ReminderMessage.Due (((DeliveryMessage.Rescheduled)source).Reminder);

			throw new InvalidOperationException (string.Format("Can not create a Due message. Can not convert from [{0}]", source.GetType().FullName));
		}

		public static DeliveryMessage.Delivered AsSent(this ReminderMessage.Due source)
		{
			return AsSent (source, SystemTime.Now());
		}

        public static DeliveryMessage.Delivered AsSent(this ReminderMessage.Due source, DateTime sentStamp)
		{
            return new DeliveryMessage.Delivered(source.ReminderId, sentStamp);
		}

        public static bool DoNotAttemptRedelivery(this DeliveryMessage.NotDelivered notDelivered)
		{
			return !notDelivered.Reminder.GiveupAfter.HasValue;
		}
	}
}

