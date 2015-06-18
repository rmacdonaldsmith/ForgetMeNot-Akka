using Akka.Dispatch;

namespace ForgetMeNot.Core.Schedule
{
    public class SchedulerMailbox : UnboundedPriorityMailbox
    {
        protected override int PriorityGenerator(object message)
        {
            if (message is Scheduler.Messages.CheckQueue)
                return 1;

            return 2;
        }
    }
}
