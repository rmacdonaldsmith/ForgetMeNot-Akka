using System;
using System.Text;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Tests
{
    public static class TestHelper
    {
        public static ReminderMessage.Schedule BuildMeAScheduleMessage()
        {
            return new ReminderMessage.Schedule(
                Guid.NewGuid(),
                SystemTime.UtcNow(),
                "http://delivery/url",
                "application/json",
                ReminderMessage.ContentEncodingEnum.utf8,
                ReminderMessage.TransportEnum.http, 
                Encoding.UTF8.GetBytes("hello world"),
                0);
        }

        public static ReminderMessage.Schedule BuildMeAScheduleMessage(DateTime dueTime)
        {
            return new ReminderMessage.Schedule(
                Guid.NewGuid(),
                dueTime,
                "http://delivery/url",
                "application/json",
                ReminderMessage.ContentEncodingEnum.utf8,
                ReminderMessage.TransportEnum.http,
                Encoding.UTF8.GetBytes("hello world"),
                0);
        }

        public static ReminderMessage.Schedule WithRetry(this ReminderMessage.Schedule schedule, int attempts, TimeSpan retryPeriod)
        {
            return new ReminderMessage.Schedule(
                schedule.ReminderId,
                schedule.DueAt,
                schedule.DeliveryUrl,
                schedule.ContentType,
                schedule.ContentEncoding,
                schedule.Transport,
                schedule.Payload,
                attempts,
                schedule.DueAt + retryPeriod,
                schedule.Tag);
        }

        public static ReminderMessage.Schedule WithIdentity(this ReminderMessage.Schedule schedule, Guid reminderId)
        {
            return new ReminderMessage.Schedule(
                reminderId,
                schedule.DueAt,
                schedule.DeliveryUrl,
                schedule.ContentType,
                schedule.ContentEncoding,
                schedule.Transport,
                schedule.Payload,
                schedule.MaxRetries,
                schedule.DueAt,
                schedule.Tag);
        }
    }
}
