namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using System.Reflection;
    using Handlers;
    using Module = Autofac.Module;

    public sealed class MediatRModule : Module
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
        }
    }
}
