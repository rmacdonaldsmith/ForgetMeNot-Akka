using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using ForgetMeNot.Messages;

namespace ForgetMeNot.Core.Startup
{
    public class StartupRouter : 
        TypedActor,
        IHandle<ReminderMessage.Schedule>,
        IHandle<ReminderMessage.Cancel>,
        IHandle<DeliveryMessage.NotDelivered>
    {
        private readonly IActorRef _schduler;
        private readonly IActorRef _cancellationFilter;
        private readonly IActorRef _notDeliveredPm;

        public StartupRouter(IActorRef schduler, IActorRef cancellationFilter, IActorRef notDeliveredPm)
        {
            _schduler = schduler;
            _cancellationFilter = cancellationFilter;
            _notDeliveredPm = notDeliveredPm;
        }

        public void Handle(ReminderMessage.Schedule message)
        {
            throw new NotImplementedException();
        }

        public void Handle(ReminderMessage.Cancel message)
        {
            throw new NotImplementedException();
        }

        public void Handle(DeliveryMessage.NotDelivered message)
        {
            throw new NotImplementedException();
        }
    }
}
