using System;
using System.Data;

namespace ForgetMeNot.Core.Journaler
{
    public interface ICommandFactory
    {
        IDbCommand GetCancellationsCommand(DateTime since);

        IDbCommand GetCurrentRemindersCommand();

        IDbCommand GetUndeliveredRemindersCommand();

        IDbCommand BuildWriteCommand<TMessage>(TMessage message) where TMessage : class ;
    }
}
