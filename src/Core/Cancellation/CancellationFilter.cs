using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;

namespace ForgetMeNot.Core.Cancellation
{
    public class CancellationFilter : TypedActor,
        IHandle<ReminderMessage.Cancel>,
        IHandle<ReminderMessage.Due>
    {
        private readonly HashSet<ReminderMessage.Cancel> _cancellations;
        private readonly IActorRef _innerHandler;
        private readonly ILog Logger = LogManager.GetLogger(typeof(CancellationFilter));

        public CancellationFilter(IActorRef innerHandler)
        {
            var comparer = new ReminderMessage.EqualityComparer<ReminderMessage.Cancel>(
                               c => c.ReminderId.GetHashCode(),
                               (x, y) => x.ReminderId == y.ReminderId);
            _cancellations = new HashSet<ReminderMessage.Cancel>(comparer);

            Ensure.NotNull(innerHandler, "innerHandler");

            _innerHandler = innerHandler;
        }

        public void Handle(ReminderMessage.Cancel msg)
        {
            if (_cancellations.Any(x => x.ReminderId == msg.ReminderId) == false)
            {
                _cancellations.Add(msg);
                Logger.Info(string.Format("Cancellation for reminder [{0}] added to cancellation list", msg.ReminderId));
            }
        }

        public void Handle(ReminderMessage.Due due)
        {
            var found = _cancellations.SingleOrDefault(x => x.ReminderId == due.ReminderId);

            if (found == null)
                _innerHandler.Tell(due);
            else
            {
                _cancellations.Remove(found);
                Logger.Info(string.Format("Cancelled Reminder [{0}] found and removed from cancellation list", due.ReminderId));
            }
        }

        public static Func<IActorRef, Props> ActorProps 
        {
            get { return receiver => Props.Create(() => new CancellationFilter(receiver)); }
        }
    }
}

