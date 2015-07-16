using System;
using System.Configuration;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using Akka.DI.CastleWindsor;
using Akka.DI.Core;
using Castle.MicroKernel.Registration;
using ForgetMeNot.API.HTTP.CustomSerializers;
using ForgetMeNot.Common;
using ForgetMeNot.Core.Cancellation;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Core.Journaler;
using ForgetMeNot.Core.Schedule;
using ForgetMeNot.Core.Startup;
using ForgetMeNot.Messages;
using Nancy.Bootstrappers.Windsor;
using log4net;
using Nancy;
using Newtonsoft.Json;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;
using RestSharp;

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

            existingContainer.Install(new NancyApiInstaller());
            existingContainer.Install(new ActorSystemInstaller());

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