namespace StreetNameRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using StreetName.Bosa;
    using StreetName.Count;
    using StreetName.Detail;
    using StreetName.List;
    using StreetName.Sync;
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

            builder.RegisterType<SyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            if (_useProjectionsV2Toggle)
            {
                builder.RegisterType<ListHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<DetailHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<CountHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<BosaHandlerV2>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<ListHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<DetailHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<CountHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
                builder.RegisterType<BosaHandler>()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
