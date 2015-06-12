using ForgetMeNot.Common;
using ForgetMeNot.Router;

namespace ForgetMeNot.Messages
{
	public static class Envelopes
	{
		public class HttpDelivery<T> : IMessage where T : class, IMessage
		{
			public T Reminder { get; private set; }

			public HttpDelivery (T toBeSent)
			{
				Ensure.NotNull(toBeSent, "toBeSent");
				Reminder = toBeSent;
			}
		}

		public class RabbitMqDelivery<T> : IMessage where T : class, IMessage
		{
			public T Reminder { get; private set; }

			public RabbitMqDelivery (T toBeSent)
			{
				Ensure.NotNull(toBeSent, "toBeSent");
				Reminder = toBeSent;
			}
		}

		public class Journaled<T> : IMessage where T : class, IMessage
		{
			private readonly T _inner;

			public T Message {
				get {return _inner;}
			}

			public Journaled (T journaledMessage)
			{
				Ensure.NotNull (journaledMessage, "journaledMessage");

				_inner = journaledMessage;
			}
		}
	}
}

