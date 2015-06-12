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
	}
}

