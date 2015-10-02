using System;

namespace ForgetMeNot.Messages
{
	public static class SystemMessage
	{
		public class BeginInitialization
		{
			//empty
		}

		public class InitializationCompleted
		{
		    public bool Successful { get; set; }
		}

        public class Start
        {
            //empty!
        }

		public class ShutDown
		{
			//empty!
		}

	    public class InitializationFailed
	    {
	        public string Message { get; private set; }
	        public Exception Error { get; private set; }

	        public InitializationFailed(string message, Exception error)
	        {
	            Message = message;
	            Error = error;
	        }
	    }
	}
}

