using System;
using System.Data;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using Npgsql;

namespace ForgetMeNot.Core.Postgres
{
	public class UndeliveredRemindersReplayer : IReplayEvents
	{
		private readonly ICommandFactory _commandFactory;
		private readonly Func<IDataReader, ReminderMessage.Undelivered> _undeliveredMapper;
		private readonly string _connectionString;

		public UndeliveredRemindersReplayer (
			ICommandFactory commandFactory, 
			string connectionString,
			Func<IDataReader, ReminderMessage.Undelivered> undeliveredMapper = null)
		{
			Ensure.NotNull (commandFactory, "commandFactory");
			Ensure.NotNullOrEmpty (connectionString, "connectionString");

			_commandFactory = commandFactory;
			_connectionString = connectionString;

			if (undeliveredMapper == null)
				_undeliveredMapper = UndeliveredMapper;
			else
				_undeliveredMapper = undeliveredMapper;
		}

		//hmm, this generic is not quite right, nothing is using T, there is no variance - need to take a closer look...
		public IObservable<T> Replay<T> (DateTime from)
		{
			var connection = new NpgsqlConnection (_connectionString);
			var command = _commandFactory.GetUndeliveredRemindersCommand ();
			return (IObservable<T>)command.ExecuteAsObservable (connection, _undeliveredMapper);
		}

		public static Func<IDataReader, ReminderMessage.Undelivered> UndeliveredMapper {
			get {
				return (reader) => {
					var journaledReminder = CurrentRemindersReplayer.ScheduleMap (reader);
					return new ReminderMessage.Undelivered (journaledReminder.Message, reader.Get<string> ("undelivered_reason"));
				};
			}
		}
	}
}

