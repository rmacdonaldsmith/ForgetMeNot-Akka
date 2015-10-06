using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;

namespace ForgetMeNot.API.HTTP.Tests
{
    [TestFixture]
    public class WhenScheduling
    {
        [Test]
        public void Should_schedule_the_reminder()
        {
            var service = new Browser(configurator => { });
        }
    }
}
