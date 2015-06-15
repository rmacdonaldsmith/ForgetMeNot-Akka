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

        public class HowManyUndeliveredRemindersDoYouHave
        {}

        public class HowManyUndeliveredRemindersDoYouHaveResponse
        {
            public int Count { get; private set; }

            public HowManyUndeliveredRemindersDoYouHaveResponse(int count)
            {
                Count = count;
            }
        }
    }
}
