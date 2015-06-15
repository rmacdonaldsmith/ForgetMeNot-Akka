using Akka.Actor;
using Akka.DI.CastleWindsor;
using Castle.MicroKernel.Registration;
using ForgetMeNot.API.HTTP.CustomSerializers;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using Nancy.Bootstrappers.Windsor;
using log4net;
using Nancy;
using Newtonsoft.Json;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;

namespace ForgetMeNot.API.HTTP.BootStrap
{
	public class BootStrapper : WindsorNancyBootstrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger("ReminderService.API.HTTP.Request");
		private readonly string _serviceInstanceId;

		public BootStrapper(string serviceInstanceId)
		{
			Ensure.NotNullOrEmpty(serviceInstanceId, "serviceInstanceId");
			_serviceInstanceId = serviceInstanceId;
		}

        protected override void RequestStartup(Castle.Windsor.IWindsorContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(RequestProcessing.PreProcessing);

            pipelines.OnError.AddItemToEndOfPipeline(RequestProcessing.ErrorProcessing);

            pipelines.AfterRequest.AddItemToEndOfPipeline(RequestProcessing.PostProcessing);

            base.RequestStartup(container, pipelines, context);
        }

        protected override void ConfigureApplicationContainer(Castle.Windsor.IWindsorContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);

            Logger.Info("Configuring the Nancy HTTP API...");

            var customJsonSerializer = new ComponentRegistration<JsonSerializer>()
                .ImplementedBy<CustomJsonSerializer>();

            var hitTracker = new HitTracker(HitTrackerSettings.Instance);
            var hitTrackerRegistration = new ComponentRegistration<HitTracker>().Instance(hitTracker);


            var system = ActorSystem.Create("forgetmenot-system");
            var propsResolver = new WindsorDependencyResolver(existingContainer, system);

            existingContainer.Register(
                customJsonSerializer,
                hitTrackerRegistration
                );

            Logger.Info("Done configuring the Nancy Http API");
        }
	}
}