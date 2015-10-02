using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Cancellation
{
    public class CancellationFilter : TypedActor,
        IHandle<ReminderMessage.Cancel>,
        IHandle<ReminderMessage.Due>
    {
        private readonly HashSet<ReminderMessage.Cancel> _cancellations;
        private readonly IActorRef _deliveryRouter;

        public static Func<IActorRef, Props> ActorProps
        {
            get { return receiver => Props.Create(() => new CancellationFilter(receiver)); }
        }

        public CancellationFilter(IActorRef deliveryRouter)
        {
            var comparer = new ReminderMessage.EqualityComparer<ReminderMessage.Cancel>(
                               c => c.ReminderId.GetHashCode(),
                               (x, y) => x.ReminderId == y.ReminderId);
            _cancellations = new HashSet<ReminderMessage.Cancel>(comparer);

            Ensure.NotNull(deliveryRouter, "deliveryRouter");

            _deliveryRouter = deliveryRouter;
        }

        public void Handle(ReminderMessage.Cancel msg)
        {
            if (_cancellations.All(x => x.ReminderId != msg.ReminderId))
            {
                _cancellations.Add(msg);
            }
        }

        public void Handle(ReminderMessage.Due due)
        {
            var found = _cancellations.SingleOrDefault(x => x.ReminderId == due.ReminderId);

            if (found == null)
                _deliveryRouter.Tell(due);
            else
            {
                _cancellations.Remove(found);
            }
        }
    }
}

