using System;
using ForgetMeNot.Common;

namespace ForgetMeNot.Messages
{
    public static class DeliveryMessage
    {
        public class Delivered : ReminderMessage.IReminder
        {
            public Guid ReminderId { get; set; }
            public DateTime SentStamp { get; private set; }

            public Delivered(Guid reminderId, DateTime sentStamp)
            {
                Ensure.NotEmptyGuid(reminderId, "reminderId");

                ReminderId = reminderId;
                SentStamp = sentStamp;
            }
        }

        public class Undeliverable : ReminderMessage.IReminder
        {
            public Guid ReminderId { get; set; }
            public ReminderMessage.Schedule Reminder { get; set; }
            public string Reason { get; set; }

            public Undeliverable(ReminderMessage.Schedule reminder, string reason)
            {
                Reminder = reminder;
                Reason = reason;
                ReminderId = reminder.ReminderId;
            }
        }

        public class Rescheduled : ReminderMessage.ISchedulable
        {
            public ReminderMessage.Schedule Reminder { get; set; }
            public DateTime DueAt { get; set; }
            public Guid ReminderId { get; set; }

            public Rescheduled(ReminderMessage.Schedule original, DateTime rescheduledFor)
            {
                Reminder = original;
                DueAt = rescheduledFor;
                ReminderId = Reminder.ReminderId;
            }
        }

        public class SentToDeadLetter : Delivered
        {
            public SentToDeadLetter(Guid reminderId, DateTime sentStamp)
                : base(reminderId, sentStamp)
            {
                //empty	
            }
        }

        public class NotDelivered : ReminderMessage.IReminder
        {
            public Guid ReminderId { get; set; }
            public ReminderMessage.Schedule Reminder { get; set; }
            public string Reason { get; set; }

            public NotDelivered(ReminderMessage.Schedule reminder, string reason)
            {
                ReminderId = reminder.ReminderId;
                Reminder = reminder;
                Reason = reason;
            }
        }
    }
}
