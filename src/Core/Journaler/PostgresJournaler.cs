using System;
using ForgetMeNot.Common;
using Npgsql;

namespace ForgetMeNot.Core.Journaler
{
	public class PostgresJournaler : IJournalEvents
	{
		private readonly ICommandFactory _commandFactory;
		private readonly string _connectionString;

		public PostgresJournaler (ICommandFactory commandFactory, string connectionString)
		{
			Ensure.NotNull (commandFactory, "commandFactory");
			Ensure.NotNullOrEmpty (connectionString, "connectionString");

			_commandFactory = commandFactory;
			_connectionString = connectionString;
		}

		public void Write (object message)
		{
			try {
				using (var connection = new NpgsqlConnection(_connectionString)) {
					using (var command = _commandFactory.BuildWriteCommand (message)) {
						command.Connection = connection;
						connection.Open();
						command.ExecuteNonQuery ();
					}
				}
			} 
			catch (Exception ex) {
				// hmmm, we will just bubble the exception up to the actor (Journaler)
                // the actor needs to be supervised by its parent 
				throw;
			}
		}
	}
}

