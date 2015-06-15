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

        protected override void ConfigureApplicationContainer(Castle.Windsor.IWindsorContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);

            Logger.Info("Configuring the Nancy HTTP API...");

            existingContainer.Register(Component.For<JsonSerializer>().ImplementedBy<CustomJsonSerializer>());

            var hitTracker = new HitTracker(HitTrackerSettings.Instance);
            existingContainer.Register(Component.For<HitTracker>().Instance(hitTracker));

            var system = ActorSystem.Create("forgetmenot-system");
            //hmmm, not sure what i can do with this
            var propsResolver = new WindsorDependencyResolver(existingContainer, system);
            existingContainer.Register(Component.For<ActorSystem>().Instance(system));

            Logger.Info("Done configuring the Nancy Http API");
        }

        protected override void ApplicationStartup(Castle.Windsor.IWindsorContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            //call base startup first, then add your additional startup stuff on top
            base.ApplicationStartup(container, pipelines);

            //initialize the forgetmenot system here
            //var startupManager = container.Resolve<ActorSystem>().ActorOf(container.Resolve<StarupActor>());
            //startupManager.Tell(new SystemMessage.BeginInitialization());
        }

        protected override void RequestStartup(Castle.Windsor.IWindsorContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(RequestProcessing.PreProcessing);

            pipelines.OnError.AddItemToEndOfPipeline(RequestProcessing.ErrorProcessing);

            pipelines.AfterRequest.AddItemToEndOfPipeline(RequestProcessing.PostProcessing);

            base.RequestStartup(container, pipelines, context);
        }
	}
}