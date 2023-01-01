namespace StreetNameRegistry.Projector.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Projections.Extract.StreetNameExtract;
    using StreetNameRegistry.Projections.Legacy.StreetNameDetail;
    using StreetNameRegistry.Projections.Legacy.StreetNameDetailV2;
    using StreetNameRegistry.Projections.Legacy.StreetNameList;
    using StreetNameRegistry.Projections.Legacy.StreetNameListV2;
    using StreetNameRegistry.Projections.Legacy.StreetNameName;
    using StreetNameRegistry.Projections.Legacy.StreetNameNameV2;
    using StreetNameRegistry.Projections.Legacy.StreetNameSyndication;
    using StreetNameRegistry.Projections.Wfs.Microsoft.StreetNameHelper;
    using ConnProjAutofac = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using ConnProjMicrosoft = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft;
    using DataDogAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using DataDogMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using EventHandlingAutofac = Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using EventHandlingMicrosoft = Be.Vlaanderen.Basisregisters.EventHandling.Microsoft;
    using ExtractAutofac = StreetNameRegistry.Projections.Extract;
    using ExtractMicrosoft = StreetNameRegistry.Projections.Extract.Microsoft;
    using LastChangedListAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using LastChangedListMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Microsoft;
    using LegacyAutofac = StreetNameRegistry.Projections.Legacy;
    using LegacyMicrosoft = StreetNameRegistry.Projections.Legacy.Microsoft;
    using SqlStreamStoreAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using SqlStreamStoreMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Microsoft;
    using StreetNameLastChangedAutofac = StreetNameRegistry.Projections.LastChangedList;
    using StreetNameLastChangedMicrosoft = StreetNameRegistry.Projections.LastChangedList.Microsoft;
    using WfsAutofac = StreetNameRegistry.Projections.Wfs;
    using WfsMicrosoft = StreetNameRegistry.Projections.Wfs.Microsoft;
    using WmsAutofac = StreetNameRegistry.Projections.Wms;
    using WmsMicrosoft = StreetNameRegistry.Projections.Wms.Microsoft;

    public sealed class ApiModule : Module, IServiceCollectionModule
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly bool _useProjectionsV2;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
            _useProjectionsV2 = Convert.ToBoolean(_configuration.GetSection(FeatureToggleOptions.ConfigurationKey)[nameof(FeatureToggleOptions.UseProjectionsV2)]);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new DataDogAutofac.DataDogModule(_configuration));
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        public void Load(IServiceCollection services)
        {
            services.RegisterModule(new DataDogMicrosoft.DataDogModule(_configuration));
            RegisterProjectionSetup(services);

            services.AddTransient<ProblemDetailsHelper>();
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new EventHandlingAutofac.EventHandlingModule(
                    typeof(DomainAssemblyMarker).Assembly,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                )
            );

            builder.RegisterModule<SqlStreamStoreAutofac.EnvelopeModule>();
            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterModule(new ProjectorModule(_configuration));

            RegisterLastChangedProjections(builder);

            if (_useProjectionsV2)
            {
                RegisterExtractProjectionsV2(builder);
                RegisterLegacyProjectionsV2(builder);
                RegisterWfsProjectionsV2(builder);
                RegisterWmsProjectionsV2(builder);
                RegisterWfsProjections(builder); //TODO: Remove when Wfs has been filled in staging
                RegisterWmsProjections(builder); //TODO: Remove when Wms has been filled in staging
            }
            else
            {
                RegisterExtractProjections(builder);
                RegisterLegacyProjections(builder);
                RegisterWfsProjections(builder);
                RegisterWmsProjections(builder);
            }
        }

        private void RegisterProjectionSetup(IServiceCollection services)
        {
            services.RegisterModule(
                new EventHandlingMicrosoft.EventHandlingModule(
                    typeof(DomainAssemblyMarker).Assembly,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                )
            );

            services.RegisterModule<SqlStreamStoreMicrosoft.EnvelopeModule>();
            services.RegisterEventstreamModule(_configuration);
            services.RegisterModule(new ProjectorModule(_configuration));

            RegisterLastChangedProjections(services);

            if (_useProjectionsV2)
            {
                RegisterExtractProjectionsV2(services);
                RegisterLegacyProjectionsV2(services);
                RegisterWfsProjectionsV2(services);
                RegisterWmsProjectionsV2(services);
                RegisterWfsProjections(services); //TODO: Remove when Wfs has been filled in staging
                RegisterWmsProjections(services); //TODO: Remove when Wms has been filled in staging
            }
            else
            {
                RegisterExtractProjections(services);
                RegisterLegacyProjections(services);
                RegisterWfsProjections(services);
                RegisterWmsProjections(services);
            }
        }

        private void RegisterExtractProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractAutofac.ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractAutofac.ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameExtractProjections, ExtractAutofac.ExtractContext>(
                    context => new StreetNameExtractProjections(context.Resolve<IOptions<ExtractAutofac.ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnProjAutofac.ConnectedProjectionSettings.Default);
        }

        private void RegisterExtractProjections(IServiceCollection services)
        {
            services.RegisterModule(
                new ExtractAutofac.ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            services
                .RegisterProjectionMigrator<ExtractMicrosoft.ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameRegistry.Projections.Extract.Microsoft.StreetNameExtract.StreetNameExtractProjections, ExtractMicrosoft.ExtractContext>(
                    context => new StreetNameRegistry.Projections.Extract.Microsoft.StreetNameExtract.StreetNameExtractProjections(context.GetRequiredService<IOptions<ExtractMicrosoft.StreetNameExtract.ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }

        private void RegisterExtractProjectionsV2(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractAutofac.ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractAutofac.ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameExtractProjectionsV2, ExtractAutofac.ExtractContext>(
                    context => new StreetNameExtractProjectionsV2(context.Resolve<IOptions<ExtractAutofac.ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnProjAutofac.ConnectedProjectionSettings.Default);
        }

        private void RegisterExtractProjectionsV2(IServiceCollection services)
        {
            services.RegisterModule(
                new ExtractAutofac.ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            services
                .RegisterProjectionMigrator<ExtractMicrosoft.ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameRegistry.Projections.Extract.Microsoft.StreetNameExtract.StreetNameExtractProjectionsV2, ExtractMicrosoft.ExtractContext>(
                    context => new StreetNameRegistry.Projections.Extract.Microsoft.StreetNameExtract.StreetNameExtractProjectionsV2(context.GetRequiredService<IOptions<ExtractMicrosoft.StreetNameExtract.ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new StreetNameLastChangedAutofac.StreetNameLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<StreetNameLastChangedAutofac.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<StreetNameLastChangedAutofac.DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameLastChangedAutofac.LastChangedProjections, LastChangedListAutofac.LastChangedListContext>(ConnProjAutofac.ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(IServiceCollection services)
        {
            services.RegisterModule(
                new StreetNameLastChangedAutofac.StreetNameLastChangedListModuleMicrosoft(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));

            services
                .RegisterProjectionMigrator<StreetNameLastChangedMicrosoft.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<StreetNameLastChangedMicrosoft.DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameLastChangedMicrosoft.LastChangedProjections, LastChangedListMicrosoft.LastChangedListContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyAutofac.LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyAutofac.LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameDetailProjections, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameListProjections, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameNameProjections, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameSyndicationProjections, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjections(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new LegacyAutofac.LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            services
                .RegisterProjectionMigrator<LegacyMicrosoft.LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameDetail.StreetNameDetailProjections, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameList.StreetNameListProjections, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameName.StreetNameNameProjections, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameSyndication.StreetNameSyndicationProjections, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyAutofac.LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyAutofac.LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameDetailProjectionsV2, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameListProjectionsV2, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameNameProjectionsV2, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameSyndicationProjections, LegacyAutofac.LegacyContext>(ConnProjAutofac.ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjectionsV2(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new LegacyAutofac.LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            services
                .RegisterProjectionMigrator<LegacyMicrosoft.LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameDetailV2.StreetNameDetailProjectionsV2, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameListV2.StreetNameListProjectionsV2, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameNameV2.StreetNameNameProjectionsV2, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default)
                .RegisterProjections<StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameSyndication.StreetNameSyndicationProjections, LegacyMicrosoft.LegacyContext>(ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }

        private void RegisterWfsProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WfsAutofac.WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnProjAutofac.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            builder
                .RegisterProjectionMigrator<WfsAutofac.WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wfs.StreetName.StreetNameHelperProjections, WfsAutofac.WfsContext>(() =>
                        new StreetNameRegistry.Projections.Wfs.StreetName.StreetNameHelperProjections(),
                    wfsProjectionSettings);
        }

        private void RegisterWfsProjections(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new WfsAutofac.WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnProjMicrosoft.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            services
                .RegisterProjectionMigrator<WfsMicrosoft.WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameHelperProjections, WfsMicrosoft.WfsContext>(() =>
                        new StreetNameHelperProjections(),
                    wfsProjectionSettings);
        }

        private void RegisterWfsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WfsAutofac.WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnProjAutofac.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            builder
                .RegisterProjectionMigrator<WfsAutofac.WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wfs.StreetNameHelperV2.StreetNameHelperV2Projections, WfsAutofac.WfsContext>(() =>
                        new StreetNameRegistry.Projections.Wfs.StreetNameHelperV2.StreetNameHelperV2Projections(),
                    wfsProjectionSettings);
        }

        private void RegisterWfsProjectionsV2(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new WfsAutofac.WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnProjMicrosoft.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            services
                .RegisterProjectionMigrator<WfsMicrosoft.WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wfs.Microsoft.StreetNameHelperV2.StreetNameHelperV2Projections, WfsMicrosoft.WfsContext>(() =>
                        new StreetNameRegistry.Projections.Wfs.Microsoft.StreetNameHelperV2.StreetNameHelperV2Projections(),
                    wfsProjectionSettings);
        }

        private void RegisterWmsProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WmsAutofac.WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnProjAutofac.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            builder
                .RegisterProjectionMigrator<WmsAutofac.WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wms.StreetName.StreetNameHelperProjections, WmsAutofac.WmsContext>(() =>
                        new StreetNameRegistry.Projections.Wms.StreetName.StreetNameHelperProjections(),
                    wmsProjectionSettings);
        }

        private void RegisterWmsProjections(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new WmsAutofac.WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnProjMicrosoft.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            services
                .RegisterProjectionMigrator<WmsMicrosoft.WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wms.Microsoft.StreetNameHelper.StreetNameHelperProjections, WmsMicrosoft.WmsContext>(() =>
                        new StreetNameRegistry.Projections.Wms.Microsoft.StreetNameHelper.StreetNameHelperProjections(),
                    wmsProjectionSettings);
        }

        private void RegisterWmsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WmsAutofac.WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnProjAutofac.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            builder
                .RegisterProjectionMigrator<WmsAutofac.WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wms.StreetNameHelperV2.StreetNameHelperV2Projections, WmsAutofac.WmsContext>(() =>
                        new StreetNameRegistry.Projections.Wms.StreetNameHelperV2.StreetNameHelperV2Projections(),
                    wmsProjectionSettings);
        }

        private void RegisterWmsProjectionsV2(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new WmsAutofac.WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnProjMicrosoft.ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            services
                .RegisterProjectionMigrator<WmsMicrosoft.WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Projections.Wms.Microsoft.StreetNameHelperV2.StreetNameHelperV2Projections, WmsMicrosoft.WmsContext>(() =>
                        new StreetNameRegistry.Projections.Wms.Microsoft.StreetNameHelperV2.StreetNameHelperV2Projections(),
                    wmsProjectionSettings);
        }
    }
}
