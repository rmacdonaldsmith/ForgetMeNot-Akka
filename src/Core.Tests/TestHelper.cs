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
    }
}
