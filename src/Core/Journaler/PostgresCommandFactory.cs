using System;
using System.Collections.Generic;
using System.Data;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using Npgsql;

namespace ForgetMeNot.Core.Journaler
{
	public class PostgresCommandFactory : ICommandFactory
	{
		private readonly IDictionary<Type, Func<object, NpgsqlCommand>> _commandSelector;

		public PostgresCommandFactory (
			IDictionary<Type, Func<object, NpgsqlCommand>> commandSelector = null)
		{
		    _commandSelector = commandSelector ?? DefaultCommandSelector;
		}

	    public IDbCommand GetCancellationsCommand (DateTime since)
	    {
	        var command = new NpgsqlCommand(string.Format(GetCancellationsCommandText, since))
	            {
	                CommandType = CommandType.Text
	            };
	        return command;
		}

		public IDbCommand GetCurrentRemindersCommand ()
		{
			return new NpgsqlCommand (GetCurrentRemindersCommandText);
		}

		public IDbCommand GetUndeliveredRemindersCommand ()
		{
			return new NpgsqlCommand (GetUndeliveredRemindersCommandText);
		}

		public IDbCommand BuildWriteCommand<T> (T message) where T : class
		{
			var command =  _commandSelector[message.GetType()](message);
			return command;
		}

		private Dictionary<Type, Func<object, NpgsqlCommand>> DefaultCommandSelector {
			get	{
				var dic = new Dictionary<Type, Func<object, NpgsqlCommand>>
				    {
				        {typeof (ReminderMessage.Cancel), WriteCancellationCommand},
				        {typeof (ReminderMessage.Schedule), WriteScheduleCommand},
				        {typeof (DeliveryMessage.Delivered), WriteSentCommand},
				        {typeof (DeliveryMessage.Undeliverable), WriteUndeliverableCommand},
				        {typeof (DeliveryMessage.NotDelivered), WriteUndeliveredCommand},
				        {typeof (DeliveryMessage.SentToDeadLetter), WriteSentToDeadLetterCommand}
				    };
			    return dic;
			}
		}

		private Func<object, NpgsqlCommand> WriteCancellationCommand {
			get {
				return msg => {
					var cancellation = msg as ReminderMessage.Cancel;
					return new NpgsqlCommand(string.Format(WriteCancellationCommandText, cancellation.ReminderId));
				};
			}
		}

		private Func<object, NpgsqlCommand> WriteScheduleCommand {
			get {
				return message => {
					var schedule = message as ReminderMessage.Schedule;
					return new NpgsqlCommand(
						string.Format(WriteScheduleReminderCommandText, schedule.ReminderId, schedule.DueAt, schedule.AsJson(), SystemTime.Now()));
				};
			}
		}

		public Func<object, NpgsqlCommand> WriteSentCommand {
			get { 
				return message => {
                    var sent = message as DeliveryMessage.Delivered;
					return new NpgsqlCommand(
						string.Format(WriteSentReminderCommandText, sent.SentStamp, sent.ReminderId)
					);
				};
			}
		}

		public Func<object, NpgsqlCommand> WriteSentToDeadLetterCommand {
			get { 
				return message => {
                    var sent = message as DeliveryMessage.SentToDeadLetter;
					return new NpgsqlCommand(
						string.Format(WriteSentReminderCommandText, sent.SentStamp, sent.ReminderId)
					);
				};
			}
		}

		public Func<object, NpgsqlCommand> WriteUndeliverableCommand {
			get {
				return message => {
                    var undeliverable = message as DeliveryMessage.Undeliverable;
					return new NpgsqlCommand(
						string.Format(WriteUndeliverableCommandText, undeliverable.ReminderId, undeliverable.Reason)
					);
				};
			}
		}

		public Func<object, NpgsqlCommand> WriteUndeliveredCommand {
			get {
				return message => {
                    var notDelivered = message as DeliveryMessage.NotDelivered;
					return new NpgsqlCommand(
						string.Format(WriteUndeliveredCommandText, notDelivered.ReminderId, notDelivered.Reason)
					);
				};
			}
		}

        //SQL Command statements
        const string GetCurrentRemindersCommandText = "SELECT * FROM public.reminders WHERE sent_time IS NULL AND cancelled = FALSE AND undelivered = FALSE AND undeliverable = FALSE";
        const string GetCancellationsCommandText = "SELECT reminder_id FROM public.reminders WHERE cancelled = TRUE AND due_time >= '{0}'";
        const string GetUndeliveredRemindersCommandText = "SELECT * FROM public.reminders WHERE undelivered = TRUE AND undeliverable = FALSE";
        const string WriteCancellationCommandText = "UPDATE public.reminders SET cancelled = TRUE WHERE reminder_id = '{0}'";
        const string WriteScheduleReminderCommandText = "INSERT INTO public.reminders VALUES ('{0}', '{1}', '{2}', NULL, FALSE, NULL, FALSE, FALSE, 1, '{3}')";
        const string WriteSentReminderCommandText = "UPDATE public.reminders SET sent_time = '{0}' WHERE reminder_id = '{1}'";
        const string WriteUndeliverableCommandText = "UPDATE public.reminders SET undeliverable = TRUE WHERE reminder_id = '{0}'";
        const string WriteUndeliveredCommandText = "UPDATE public.reminders SET undelivered = TRUE, undelivered_reason = '{1}' WHERE reminder_id = '{0}'";
	}
}

