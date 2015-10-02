using System;
using System.Data;
using ForgetMeNot.Common;
using ForgetMeNot.Core.Journaler;
using ForgetMeNot.Messages;
using Npgsql;

namespace ForgetMeNot.Core.Startup
{
	public class CancellationReplayer : IReplayEvents
	{
		private readonly ICommandFactory _commandFactory;
		private readonly Func<IDataReader, ReminderMessage.Cancel> _cancellationMapper;
		private readonly string _connectionString;

		public CancellationReplayer (
			ICommandFactory commandFactory, 
			string connectionString,
			Func<IDataReader, ReminderMessage.Cancel> cancellationMapper = null)
		{
			Ensure.NotNull (commandFactory, "commandFactory");
			Ensure.NotNullOrEmpty (connectionString, "connectionString");

			_commandFactory = commandFactory;
			_connectionString = connectionString;

			if (cancellationMapper == null)
				_cancellationMapper = CancellationMap;
			else
				_cancellationMapper = cancellationMapper;
		}

		public IObservable<T> Replay<T> (DateTime from)
		{
			var connection = new NpgsqlConnection (_connectionString);
			var command = _commandFactory.GetCancellationsCommand (from);
			return (IObservable<T>)command.ExecuteAsObservable (connection, _cancellationMapper);
		}

		public static Func<IDataReader, ReminderMessage.Cancel> CancellationMap {
			get { 
				return (reader) => new ReminderMessage.Cancel (reader.GetGuid("reminder_id"));
			}
		}
	}
}

