﻿using System.Configuration;
using Akka.Actor;
using Akka.DI.CastleWindsor;
using Akka.DI.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ForgetMeNot.Core.Cancellation;
using ForgetMeNot.Core.DeliverReminder;
using ForgetMeNot.Core.Journaler;
using ForgetMeNot.Core.Schedule;
using ForgetMeNot.Core.Startup;
using RestSharp;

namespace ForgetMeNot.API.HTTP.BootStrap
{
    public class ActorSystemInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var system = ActorSystem.Create("forgetmenot-system");
            container.Register(Component.For<ActorSystem>().Instance(system));
            var propsResolver = new WindsorDependencyResolver(container, system);
            system.AddDependencyResolver(propsResolver);

            container.Register(Component.For<IRestClient>().ImplementedBy<RestClient>().LifestyleSingleton());

            var connectionString = ConfigurationManager.ConnectionStrings["postgres"].ConnectionString;
            var journaler = new PostgresJournaler(new PostgresCommandFactory(), connectionString);
            container.Register(Component.For<IJournalEvents>().Instance(journaler).LifestyleSingleton());

            //actor registrations
            var journalerRef = system.ActorOf(Journaler.PropsFactory(container.Resolve<IJournalEvents>()));
            container.Register(Component.For<IActorRef>().Named("Journaler").Instance(journalerRef).LifestyleTransient());

            var deliveryRouterRef = system.ActorOf(
                DeliveryRouter.PropsFactory(journalerRef,
                                            HttpDelivery.PropsFactory(container.Resolve<IRestClient>()),
                                            DeadLetterDelivery.PropsFactory()));
            container.Register(
                Component.For<IActorRef>().Instance(deliveryRouterRef)
                         .Named("DeliveryRouter")
                         .LifestyleTransient());

            var schedulerRef = system.ActorOf(Scheduler.ActorProps(10000, deliveryRouterRef));
            container.Register(
                Component.For<IActorRef>()
                         .Named("Scheduler")
                         .Instance(schedulerRef)
                         .LifestyleTransient());


            container.Register(Component.For<IReplayEvents>().ImplementedBy<CancellationReplayer>().LifestyleSingleton());
            container.Register(Component.For<IReplayEvents>().ImplementedBy<CurrentRemindersReplayer>().LifestyleSingleton());
            container.Register(Component.For<IReplayEvents>().ImplementedBy<UndeliveredRemindersReplayer>().LifestyleSingleton());
            container.Register(Component.For<SystemStartManager>().Named("SystemStartManager").LifestyleTransient());

            container.Register(Component.For<CancellationFilter>().Named("CancellationFilter").LifestyleTransient());

//            container.Register(
//                Component.For<Props>()
//                    .UsingFactoryMethod(() => HttpDelivery.PropsFactory(container.Resolve<IRestClient>()))
//                    .Named("HttpDelivery")
//                    .LifestyleTransient());
//
//            container.Register(
//                Component.For<Props>()
//                    .UsingFactoryMethod(() => DeadLetterDelivery.PropsFactory())
//                    .Named("DeadLetterDelivery")
//                    .LifestyleTransient());
        }
    }
}
