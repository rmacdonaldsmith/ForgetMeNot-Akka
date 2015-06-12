using ForgetMeNot.Common;
using ForgetMeNot.DataStructures;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core
{
    public class Scheduler : ReceiveActor
    {
        private readonly ActorRef _deliveryRouter;
        private readonly MinPriorityQueue<ReminderMessage.ISchedulable> _pq;
        private bool _running;

        public Scheduler(int seedSize, ActorRef deliveryRouter)
        {
            Ensure.Nonnegative(seedSize, "seedSize");
            Ensure.NotNull(deliveryRouter, "deliveryRouter");

            _deliveryRouter = deliveryRouter;
            _pq = new MinPriorityQueue<ReminderMessage.ISchedulable>(seedSize, (a, b) => a.DueAt > b.DueAt);
        }

        public void Receive(SystemMessage.Start start)
        {
            _running = true;
            Self.Tell(new Messages.CheckQueue());
        }

        public void Receive(Messages.CheckQueue checkQueue)
        {
            while (!_pq.IsEmpty && _pq.Min().DueAt <= SystemTime.UtcNow())
            {
                var due = _pq.RemoveMin().AsDue();
                Logger.DebugFormat("Timer fired. Dequeing reminder {0}", due.ReminderId);
                _deliveryRouter.Tell(due);
            }
        }

        public class Messages
        {
            public class CheckQueue
            {
                //empty
            }
        }
    }
}
