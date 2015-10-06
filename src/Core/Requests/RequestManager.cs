using System;
using Akka.Actor;
using ForgetMeNot.Messages;
using Journal = ForgetMeNot.Core.Journaler.Journaler;

namespace ForgetMeNot.Core.Requests
{
    public class RequestManager : ReceiveActor
    {
        private readonly IActorRef _journaler;
        private readonly IActorRef _scheduler;
        private readonly IActorRef _cancellationFilter;

        public RequestManager(IActorRef journaler, IActorRef scheduler, IActorRef cancellationFilter)
        {
            _journaler = journaler;
            _scheduler = scheduler;
            _cancellationFilter = cancellationFilter;

            Receive<SystemMessage.Start>(start => BecomeStacked(Running));
        }

        public void Running()
        {
            Receive<ReminderMessage.Schedule>(async schedule =>
                {
                    await _journaler.Ask<Journal.Messages.Journaled<ReminderMessage.Schedule>>(schedule);
                    _scheduler.Tell(schedule);
                    Sender.Tell(new ReminderMessage.Scheduled());
                });

            Receive<ReminderMessage.Cancel>(cancel => _cancellationFilter.Tell(cancel));

            Receive<SystemMessage.ShutDown>(stop => UnbecomeStacked());
        }

        public static Func<IActorRef, IActorRef, IActorRef, Props> PropsFactory
        {
            get
            {
                return
                    (journaler, scheduler, cancellation) =>
                    Props.Create(() => new RequestManager(journaler, scheduler, cancellation));
            }
        }
    }
}
