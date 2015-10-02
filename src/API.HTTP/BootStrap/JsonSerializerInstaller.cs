using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ForgetMeNot.API.HTTP.CustomSerializers;
using Newtonsoft.Json;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;

namespace ForgetMeNot.API.HTTP.BootStrap
{
    public class NancyApiInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<JsonSerializer>().ImplementedBy<CustomJsonSerializer>());

            var hitTracker = new HitTracker(HitTrackerSettings.Instance);
            container.Register(Component.For<HitTracker>().Instance(hitTracker));
        }
    }
}
