using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.DataStructures;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Schedule
{
    public class Scheduler : ReceiveActor
    {
        private readonly IActorRef _deliveryRouter;
        private readonly MinPriorityQueue<ReminderMessage.ISchedulable> _pq;
        private bool _running;

        public Scheduler(int seedSize, IActorRef deliveryRouter)
        {
            Ensure.Nonnegative(seedSize, "seedSize");
            Ensure.NotNull(deliveryRouter, "deliveryRouter");

            _deliveryRouter = deliveryRouter;
            _pq = new MinPriorityQueue<ReminderMessage.ISchedulable>(seedSize, (a, b) => a.DueAt > b.DueAt);

            Receive<SystemMessage.Start>(start =>
                {
                    _running = true;
                    SetTimer();
                    BecomeStacked(Running);
                });
        }

        public void Running()
        {
            Receive<Messages.CheckQueue>(checkQ =>
                {
                    while (!_pq.IsEmpty && _pq.Min().DueAt <= SystemTime.UtcNow())
                    {
                        var due = _pq.RemoveMin().AsDue();
                        //Logger.DebugFormat("Timer fired. Dequeing reminder {0}", due.ReminderId);
                        _deliveryRouter.Tell(due);
                    }
                });

            Receive<ReminderMessage.Schedule>(schedule => _pq.Insert(schedule));

            //may not need this message type
            Receive<ReminderMessage.Rescheduled>(reschedule => _pq.Insert(reschedule));

            Receive<QueryMessage.HowBigIsYourQueue>(
                query => Sender.Tell(new QueryMessage.HowBigIsYourQueueResponse(_pq.Size)));

            Receive<SystemMessage.ShutDown>(shutDown => UnbecomeStacked()); //todo: stop sending check queue messages
        }

        private void SetTimer()
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(1000, 1000, Self, new Messages.CheckQueue(), Self);
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
