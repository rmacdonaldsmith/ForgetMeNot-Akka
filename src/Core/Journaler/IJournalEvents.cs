namespace ForgetMeNot.Core.Journaler
{
    public interface IJournalEvents
    {
        void Write(object message);
    }
}
