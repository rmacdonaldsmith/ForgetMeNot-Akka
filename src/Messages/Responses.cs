using System;
using ReminderService.Router.MessageInterfaces;

namespace ReminderService.Messages
{
	public static class Responses
	{
		//todo: make this a static class based enum?
		public enum ReminderStatusEnum
		{
			Scheduled,
			Delivered,
			Canceled,
			Undeliverable
		}

		public class CurrentReminderState : IMessage
		{
			public ReminderMessage.Schedule OriginalReminder { get; set; }
			public ReminderStatusEnum Status { get; set; }
		}
	}
}

