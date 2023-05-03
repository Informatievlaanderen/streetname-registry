namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System.Linq;
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

            builder
                .RegisterAssemblyTypes(typeof(ProposeStreetNameHandler).GetTypeInfo().Assembly)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces();
        }
    }
}
