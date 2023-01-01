namespace StreetNameRegistry.Producer.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;
    using ConnProjAutofac = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using ConnProjMicrosoft = Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft;
    using DataDogAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using DataDogMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using EventHandlingAutofac = Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using EventHandlingMicrosoft = Be.Vlaanderen.Basisregisters.EventHandling.Microsoft;
    using SqlStreamStoreAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using SqlStreamStoreMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Microsoft;

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
            builder.RegisterModule(new DataDogAutofac.DataDogModule(_configuration));

            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingAutofac.EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))

                .RegisterModule<SqlStreamStoreAutofac.EnvelopeModule>()

                .RegisterEventstreamModule(_configuration)

                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ProducerModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var connectedProjectionSettings = ConnProjAutofac.ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnProjAutofac.ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerProjections, ProducerContext>(() =>
                {
                    var bootstrapServers = _configuration["Kafka:BootstrapServers"];
                    var topic = $"{_configuration[ProducerProjections.StreetNameTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerProjections.StreetNameTopicKey}");
                    var producerOptions = new ProducerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            true,
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                        .ConfigureEnableIdempotence();
                    if (!string.IsNullOrEmpty(_configuration["Kafka:SaslUserName"])
                        && !string.IsNullOrEmpty(_configuration["Kafka:SaslPassword"]))
                    {
                        producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            _configuration["Kafka:SaslUserName"],
                            _configuration["Kafka:SaslPassword"]));
                    }
                    return new ProducerProjections(new Producer(producerOptions));
                }, connectedProjectionSettings);
        }

        public void Load(IServiceCollection services)
        {
            services.RegisterModule(new DataDogMicrosoft.DataDogModule(_configuration));

            RegisterProjectionSetup(services);

            services.AddTransient<ProblemDetailsHelper>();
        }

        private void RegisterProjectionSetup(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new EventHandlingMicrosoft.EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))

                .RegisterModule<SqlStreamStoreMicrosoft.EnvelopeModule>()

                .RegisterEventstreamModule(_configuration)

                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(services);
        }

        private void RegisterProjections(IServiceCollection services)
        {
            services
                .RegisterModule(
                    new ProducerModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var connectedProjectionSettings = ConnProjMicrosoft.ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnProjMicrosoft.ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            services
                .RegisterProjectionMigrator<Microsoft.ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<Microsoft.ProducerProjections, Microsoft.ProducerContext>(() =>
                {
                    var bootstrapServers = _configuration["Kafka:BootstrapServers"];
                    var topic = $"{_configuration[ProducerProjections.StreetNameTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerProjections.StreetNameTopicKey}");
                    var producerOptions = new ProducerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            true,
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                        .ConfigureEnableIdempotence();
                    if (!string.IsNullOrEmpty(_configuration["Kafka:SaslUserName"])
                        && !string.IsNullOrEmpty(_configuration["Kafka:SaslPassword"]))
                    {
                        producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            _configuration["Kafka:SaslUserName"],
                            _configuration["Kafka:SaslPassword"]));
                    }
                    return new Microsoft.ProducerProjections(new Producer(producerOptions));
                }, connectedProjectionSettings);
        }
    }
}
