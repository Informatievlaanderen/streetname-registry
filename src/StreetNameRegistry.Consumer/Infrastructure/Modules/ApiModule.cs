namespace StreetNameRegistry.Consumer.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Projections;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;
    using ConsumerAutofac = StreetNameRegistry.Consumer;
    using ConsumerMicrosoft = StreetNameRegistry.Consumer.Microsoft;
    using ConnProjAutofac = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using ConnProjMicrosoft = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft;
    using DataDogAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using DataDogMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using EventHandlingAutofac = Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using EventHandlingMicrosoft = Be.Vlaanderen.Basisregisters.EventHandling.Microsoft;
    using ProjectionHandlingAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using ProjectionHandlingMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Microsoft;

    public class ApiModule : Module, IServiceCollectionModule
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
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            builder
                .RegisterModule(new DataDogAutofac.DataDogModule(_configuration))

                .RegisterModule<ProjectionHandlingAutofac.EnvelopeModule>()

                .RegisterModule(new EventHandlingAutofac.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))

                .RegisterModule(new CommandHandlingModule(_configuration));

            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterSnapshotModule(_configuration);

            builder
                .RegisterProjectionMigrator<ConsumerContextFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<MunicipalityConsumerProjection, ConsumerAutofac.ConsumerContext>(
                    context => new MunicipalityConsumerProjection(),
                    ConnProjAutofac.ConnectedProjectionSettings.Default);

            builder.Populate(_services);
        }

        public void Load(IServiceCollection services)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            services
                .RegisterModule(new DataDogMicrosoft.DataDogModule(_configuration))

                .RegisterModule<ProjectionHandlingMicrosoft.EnvelopeModule>()

                .RegisterModule(new EventHandlingMicrosoft.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))

                .RegisterModule(new CommandHandlingModule(_configuration));

            services.RegisterEventstreamModule(_configuration);
            services.RegisterSnapshotModule(_configuration);

            services
                .RegisterProjectionMigrator<ConsumerMicrosoft.ConsumerContextFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameRegistry.Consumer.Microsoft.Projections.MunicipalityConsumerProjection, ConsumerMicrosoft.ConsumerContext>(
                    context => new StreetNameRegistry.Consumer.Microsoft.Projections.MunicipalityConsumerProjection(),
                    ConnProjMicrosoft.ConnectedProjectionSettings.Default);
        }
    }
}
