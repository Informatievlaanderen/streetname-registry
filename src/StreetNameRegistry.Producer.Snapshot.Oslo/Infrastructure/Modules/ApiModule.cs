namespace StreetNameRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;

    public class ApiModule : Module
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

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))

                .RegisterModule<EnvelopeModule>()

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

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerProjections, ProducerContext>(c =>
                    {
                        //TODO: Needed when removed streetname is implemented
                        //var osloNamespace = _configuration["OsloNamespace"];
                        //osloNamespace = osloNamespace.TrimEnd('/');

                        var bootstrapServers = _configuration["Kafka:BootstrapServers"]!;
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
                                _configuration["Kafka:SaslUserName"]!,
                                _configuration["Kafka:SaslPassword"]!));
                        }

                        var osloProxy = c.Resolve<IOsloProxy>();
                        return new ProducerProjections(
                            new Producer(producerOptions),
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                osloProxy,
                                SnapshotManagerOptions.Create(
                                    _configuration["RetryPolicy:MaxRetryWaitIntervalSeconds"]!,
                                    _configuration["RetryPolicy:RetryBackoffFactor"]!)),
                            _configuration["OsloNamespace"]!,
                            osloProxy);
                    },
                    connectedProjectionSettings);
        }
    }
}
