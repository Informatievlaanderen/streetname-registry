namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System.Reflection;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Handlers;
    using Handlers.Sqs.Handlers;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Module = Autofac.Module;

    public sealed class MediatRModule : Module, IServiceCollectionModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });

            builder.RegisterAssemblyTypes(typeof(ProposeStreetNameHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(ProposeStreetNameSqsHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();
        }

        public void Load(IServiceCollection services)
        {
            services.AddMediatR(typeof(ProposeStreetNameHandler).GetTypeInfo().Assembly, typeof(ProposeStreetNameSqsHandler).GetTypeInfo().Assembly);
        }
    }
}
