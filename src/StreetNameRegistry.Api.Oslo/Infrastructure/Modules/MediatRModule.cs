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
        private readonly UseProjectionsV2Toggle _useProjectionsV2Toggle;

        public MediatRModule(UseProjectionsV2Toggle useProjectionsV2Toggle)
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

            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                builder.RegisterGeneric(typeof(OsloGetHandlerV2))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(OsloListHandlerV2))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(OsloCountHandlerV2))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterGeneric(typeof(OsloGetHandler))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(OsloListHandler))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(OsloCountHandler))
                    .As(typeof(RequestHandler<>))
                    .InstancePerLifetimeScope();
            }
        }
    }
}
