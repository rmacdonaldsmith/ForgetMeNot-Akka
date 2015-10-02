using System;
using System.Data;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core
{
	public static class RxExtensions
	{
		public static IObservable<T> ExecuteAsObservable<T>(this IDbCommand command, IDbConnection connection, Func<IDataReader, T> mapper)
		{
			return Observable.Create<T> (observer => {
				using (connection) {
					using(command) {
						try {
							command.Connection = connection;
							connection.Open();
							using (var reader = command.ExecuteReader()){
								while(reader.Read())
								{
									var value = mapper(reader);
									observer.OnNext(value);
								}
							}
						}
						catch (Exception ex) {
							observer.OnError(ex);
						}
					}
				}
				observer.OnCompleted();
				return Disposable.Empty;
			});
		}



		public static Func<IDataReader, ReminderMessage.Cancel> CancelMapper()
		{
			return (reader) => {
				return new ReminderMessage.Cancel(reader.GetGuid(0));
			};
		}
	}
}

