using System;

namespace ForgetMeNot.Common
{
	public class ReminderUndeliverableException<T> : Exception
	{
		public T Reminder { get; set; }

		public ReminderUndeliverableException (T undeliverable)
			: base()
		{
			Reminder = undeliverable;
		}

		public ReminderUndeliverableException (T undeliverable, string message)
			: base(message)
		{
			Reminder = undeliverable;
		}
	}
}

