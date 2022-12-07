namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using StreetName.Count;
    using StreetName.Detail;
    using Module = Autofac.Module;
    using OsloCountHandlerV2 = StreetName.Count.OsloCountHandlerV2;
    using OsloListHandler = StreetName.List.OsloListHandler;
    using OsloListHandlerV2 = StreetName.List.OsloListHandlerV2;

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
                builder.RegisterType<OsloDetailHandlerV2>()
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
                builder.RegisterType<OsloDetailHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<OsloCountHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
