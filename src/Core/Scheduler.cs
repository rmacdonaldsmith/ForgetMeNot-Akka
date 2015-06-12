using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ForgetMeNot.DataStructures;

namespace ForgetMeNot.Core.Tests
{
    public class Scheduler : ReceiveActor
    {
        private readonly MinPriorityQueue<ISchedulable> _pq;
    }
}
