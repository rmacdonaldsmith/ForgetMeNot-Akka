using System;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Tests.Startup
{
	public class FakeReplayer : IReplayEvents
	{
		private readonly Func<DateTime, IObservable<ReminderMessage.IReminder>> _generator;

		public FakeReplayer (Func<DateTime, IObservable<ReminderMessage.IReminder>> generator)
		{
			_generator = generator;
		}

		public IObservable<T> Replay<T> (DateTime from)
		{
			return (IObservable<T>) _generator (@from);
		}
	}
}

