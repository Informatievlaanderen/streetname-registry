namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using FeatureToggles;
    using Handlers.Count;
    using Handlers.Get;
    using Handlers.List;
    using MediatR;
    using Module = Autofac.Module;

    public sealed class MediatRModule : Module
    {
        private readonly bool _useProjectionsV2Toggle;

        public MediatRModule(bool useProjectionsV2Toggle)
        {
            _useProjectionsV2Toggle = useProjectionsV2Toggle;
        }

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

            if (_useProjectionsV2Toggle)
            {
                builder.RegisterType<OsloListHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<OsloListHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<OsloCountHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<OsloListHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<OsloListHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<OsloListHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
