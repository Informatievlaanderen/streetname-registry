namespace StreetNameRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using SqlStreamStore;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Projections.Elastic;
    using StreetNameRegistry.Projections.Elastic.Infrastructure;
    using StreetNameRegistry.Projections.Elastic.StreetNameList;
    using StreetNameRegistry.Projections.Extract;
    using StreetNameRegistry.Projections.Extract.StreetNameExtract;
    using StreetNameRegistry.Projections.Integration;
    using StreetNameRegistry.Projections.Integration.Infrastructure;
    using StreetNameRegistry.Projections.Integration.Merger;
    using StreetNameRegistry.Projections.LastChangedList;
    using StreetNameRegistry.Projections.Legacy;
    using StreetNameRegistry.Projections.Legacy.StreetNameDetailV2;
    using StreetNameRegistry.Projections.Legacy.StreetNameListV2;
    using StreetNameRegistry.Projections.Legacy.StreetNameSyndication;
    using StreetNameRegistry.Projections.Syndication;
    using StreetNameRegistry.Projections.Wfs;
    using StreetNameRegistry.Projections.Wfs.StreetNameHelperV2;
    using StreetNameRegistry.Projections.Wms;

    public sealed class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Register(c =>
                    new LastChangedListCacheValidator(
                        c.Resolve<LegacyContext>(),
                        typeof(StreetNameDetailProjectionsV2).FullName))
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new EventHandlingModule(
                    typeof(DomainAssemblyMarker).Assembly,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                )
            );

            builder.RegisterModule<EnvelopeModule>();
            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterModule(new ProjectorModule(_configuration));

            RegisterLastChangedProjections(builder);

            RegisterExtractProjectionsV2(builder);
            RegisterLegacyProjectionsV2(builder);
            RegisterWfsProjectionsV2(builder);
            RegisterWmsProjectionsV2(builder);

            if (_configuration.GetSection("Integration").GetValue("Enabled", false))
                RegisterIntegrationProjections(builder);

            RegisterElasticProjections(builder);
        }

        private void RegisterIntegrationProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new IntegrationModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<IntegrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameLatestItemProjections, IntegrationContext>(
                    context => new StreetNameLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameVersionProjections, IntegrationContext>(
                    context => new StreetNameVersionProjections(
                        context.Resolve<IOptions<IntegrationOptions>>(),
                        context.Resolve<IEventsRepository>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameMergerItemProjections, IntegrationContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterExtractProjectionsV2(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameExtractProjectionsV2, ExtractContext>(
                    context => new StreetNameExtractProjectionsV2(
                        context.Resolve<IReadonlyStreamStore>(),
                        context.Resolve<EventDeserializer>(),
                        context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new StreetNameLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _services,
                    _loggerFactory));
        }

        private void RegisterLegacyProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameDetailProjectionsV2, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameListProjectionsV2, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameSyndicationProjections, LegacyContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterWfsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            builder
                .RegisterProjectionMigrator<WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameHelperV2Projections, WfsContext>(() =>
                        new StreetNameHelperV2Projections(),
                    wfsProjectionSettings);
        }

        private void RegisterWmsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            builder
                .RegisterProjectionMigrator<WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameRegistry.Projections.Wms.StreetNameHelperV2.StreetNameHelperV2Projections, WmsContext>(() =>
                        new StreetNameRegistry.Projections.Wms.StreetNameHelperV2.StreetNameHelperV2Projections(),
                    wmsProjectionSettings);
        }

        private void RegisterElasticProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ElasticRunnerModule(
                        _configuration,
                        _services,
                        _loggerFactory))
                .RegisterModule(new ElasticModule(_configuration));

            builder
                .RegisterModule(
                    new SyndicationModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            builder
                .RegisterProjectionMigrator<ElasticRunnerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameListProjections, ElasticRunnerContext>(c =>
                        new StreetNameListProjections(
                            c.Resolve<IStreetNameListElasticClient>(),
                            c.Resolve<IDbContextFactory<SyndicationContext>>()),
                    ConnectedProjectionSettings.Configure(x => { x.ConfigureCatchUpUpdatePositionMessageInterval(1); }))
                ;
        }
    }
}
