namespace StreetNameRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FeatureToggles;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Options;
    using MediatR;
    using Options;
    using HandlersBosaAutofac = StreetName.Bosa;
    using HandlersCountAutofac = StreetName.Count;
    using HandlersDetailAutofac = StreetName.Detail;
    using HandlersListAutofac = StreetName.List;
    using HandlersSyncAutofac = StreetName.Sync;
    using HandlersBosaMicrosoft = Microsoft.StreetName.Bosa;
    using HandlersCountMicrosoft = Microsoft.StreetName.Count;
    using HandlersDetailMicrosoft = Microsoft.StreetName.Detail;
    using HandlersListMicrosoft = Microsoft.StreetName.List;
    using HandlersSyncMicrosoft = StreetName.Sync;
    using LegacyAutofac = Projections.Legacy;
    using LegacyMicrosoft = Projections.Legacy.Microsoft;
    using SyndicationAutofac = Projections.Syndication;
    using SyndicationMicrosoft = Projections.Syndication.Microsoft;

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

            builder.RegisterType<HandlersSyncAutofac.SyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersListAutofac.ListHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<HandlersListAutofac.ListRequest, HandlersListAutofac.StreetNameListResponse>)
                    new HandlersListAutofac.ListHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersDetailAutofac.DetailHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<HandlersDetailAutofac.DetailRequest, HandlersDetailAutofac.StreetNameResponse>)
                    new HandlersDetailAutofac.DetailHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersCountAutofac.CountHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>());
                }

                return (IRequestHandler<HandlersCountAutofac.CountRequest, TotaalAantalResponse>)
                    new HandlersCountAutofac.CountHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersBosaAutofac.BosaHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<HandlersBosaAutofac.BosaStreetNameRequest, HandlersBosaAutofac.StreetNameBosaResponse>)
                    new HandlersBosaAutofac.BosaHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();
        }

        public void Load(IServiceCollection services)
        {
            services.AddMediatR(typeof(HandlersSyncMicrosoft.SyndicationHandler).Assembly);

            services.AddScoped<IRequestHandler<HandlersListMicrosoft.ListRequest, HandlersListMicrosoft.StreetNameListResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersListMicrosoft.ListHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>())
                : new HandlersListMicrosoft.ListHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>()));

            services.AddScoped<IRequestHandler<HandlersDetailMicrosoft.DetailRequest, HandlersDetailMicrosoft.StreetNameResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersDetailMicrosoft.DetailHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>())
                : new HandlersDetailMicrosoft.DetailHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>()));

            services.AddScoped<IRequestHandler<HandlersCountMicrosoft.CountRequest, TotaalAantalResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersCountMicrosoft.CountHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>())
                : new HandlersCountMicrosoft.CountHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>()));

            services.AddScoped<IRequestHandler<HandlersBosaMicrosoft.BosaStreetNameRequest, HandlersBosaMicrosoft.StreetNameBosaResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersBosaMicrosoft.BosaHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>())
                : new HandlersBosaMicrosoft.BosaHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>()));
        }
    }
}
