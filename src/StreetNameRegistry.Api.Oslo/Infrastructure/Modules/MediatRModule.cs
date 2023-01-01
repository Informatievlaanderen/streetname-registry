namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Abstractions.Infrastructure.Options;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FeatureToggles;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Options;
    using MediatR;
    using HandlersCountAutofac = StreetName.Count;
    using HandlersDetailAutofac = StreetName.Detail;
    using HandlersListAutofac = StreetName.List;
    using HandlersCountMicrosoft = Microsoft.StreetName.Count;
    using HandlersDetailMicrosoft = Microsoft.StreetName.Detail;
    using HandlersListMicrosoft = Microsoft.StreetName.List;
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

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersListAutofac.OsloListHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<HandlersListAutofac.OsloListRequest, HandlersListAutofac.StreetNameListOsloResponse>)
                    new HandlersListAutofac.OsloListHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersDetailAutofac.OsloDetailHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<HandlersDetailAutofac.OsloDetailRequest, HandlersDetailAutofac.StreetNameOsloResponse>)
                    new HandlersDetailAutofac.OsloDetailHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return new HandlersCountAutofac.OsloCountHandlerV2(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>());
                }

                return (IRequestHandler<HandlersCountAutofac.OsloCountRequest, TotaalAantalResponse>)
                    new HandlersCountAutofac.OsloCountHandler(
                        c.Resolve<LegacyAutofac.LegacyContext>(),
                        c.Resolve<SyndicationAutofac.SyndicationContext>());
            }).InstancePerLifetimeScope();
        }

        public void Load(IServiceCollection services)
        {
            services.AddMediatR(typeof(HandlersListAutofac.OsloListHandlerV2).Assembly);

            services.AddScoped<IRequestHandler<HandlersListMicrosoft.OsloListRequest, HandlersListMicrosoft.StreetNameListOsloResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersListMicrosoft.OsloListHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>())
                : new HandlersListMicrosoft.OsloListHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>()));

            services.AddScoped<IRequestHandler<HandlersDetailMicrosoft.OsloDetailRequest, HandlersDetailMicrosoft.StreetNameOsloResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersDetailMicrosoft.OsloDetailHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>())
                : new HandlersDetailMicrosoft.OsloDetailHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>(),
                    _.GetRequiredService<IOptions<ResponseOptions>>()));

            services.AddScoped<IRequestHandler<HandlersCountMicrosoft.OsloCountRequest, TotaalAantalResponse>>(_ => _.GetRequiredService<UseProjectionsV2Toggle>().FeatureEnabled
                ? new HandlersCountMicrosoft.OsloCountHandlerV2(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>())
                : new HandlersCountMicrosoft.OsloCountHandler(
                    _.GetRequiredService<LegacyMicrosoft.LegacyContext>(),
                    _.GetRequiredService<SyndicationMicrosoft.SyndicationContext>()));
        }
    }
}
