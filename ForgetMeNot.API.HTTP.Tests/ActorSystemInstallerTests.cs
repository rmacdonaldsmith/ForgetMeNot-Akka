using Castle.Core;
using Castle.Windsor;
using ForgetMeNot.API.HTTP.BootStrap;
using ForgetMeNot.Core.Journaler;
using NUnit.Framework;

namespace ForgetMeNot.API.HTTP.Tests
{
    [TestFixture]
    public class ActorSystemInstallerTests
    {
        private IWindsorContainer container;

        [TestFixtureSetUp]
        public void InitializeContainer()
        {
            container = new WindsorContainer().Install(new ActorSystemInstaller());
        }

        [Test]
        public void Can_resolve_the_Journaller()
        {
            var journaller = container.Kernel.Resolve<Journaler>("Journaler");

            Assert.IsNotNull(journaller);
            Assert.AreEqual(LifestyleType.Transient, container.Kernel.GetHandler("Journaller").ComponentModel.LifestyleType);
        }
    }
}
