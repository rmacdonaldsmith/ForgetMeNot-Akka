using ForgetMeNot.Common;

namespace ForgetMeNot.Messages
{
	public static class Envelopes
	{
		public class HttpDelivery<T> where T: class 
		{
			public T Reminder { get; private set; }

			public HttpDelivery (T toBeSent)
			{
				Ensure.NotNull(toBeSent, "toBeSent");
				Reminder = toBeSent;
			}
		}

		public class RabbitMqDelivery<T> where T : class
		{
			public T Reminder { get; private set; }

			public RabbitMqDelivery (T toBeSent)
			{
				Ensure.NotNull(toBeSent, "toBeSent");
				Reminder = toBeSent;
			}
		}

		public class Journaled<T> where T : class
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

