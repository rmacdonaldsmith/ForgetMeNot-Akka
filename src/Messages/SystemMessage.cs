using ForgetMeNot.Router;

namespace ForgetMeNot.Messages
{
	public static class SystemMessage
	{
		public class BeginInitialization : IMessage
		{
			//empty
		}

		public class InitializationCompleted : IMessage
		{
		    public bool Successful { get; set; }
		}

        public class Start : IMessage
        {
            //empty!
        }

		public class ShutDown : IMessage
		{
			//empty!
		}
	}
}

