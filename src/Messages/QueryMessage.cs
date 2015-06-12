namespace ForgetMeNot.Messages
{
    public static class QueryMessage
    {
        public class HowBigIsYourQueue
        {
        }

        public class HowBigIsYourQueueResponse
        {
            public int Size { get; private set; }

            public HowBigIsYourQueueResponse(int size)
            {
                Size = size;
            }
        }
    }
}
