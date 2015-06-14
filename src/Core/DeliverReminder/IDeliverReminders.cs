using System;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.DeliverReminder
{
	public interface IDeliverReminders
	{
		void Send (ReminderMessage.Schedule dueReminder, string url, Action<ReminderMessage.Schedule> onSuccessfulSend, Action<ReminderMessage.Schedule, string> onFailedSend);
	}
}

