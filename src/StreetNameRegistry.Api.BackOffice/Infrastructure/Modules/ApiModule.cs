namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer;
    using Handlers.Sqs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;

    public sealed class ApiModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";

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

            _services.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder.RegisterModule(new IdempotencyModule(
                _services,
                _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                _loggerFactory));

            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly,
                eventSerializerSettings));

            builder.RegisterModule(new EnvelopeModule());
            builder.RegisterModule(new SequenceModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory));
            builder.RegisterModule(new MediatRModule());
            builder.RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]));
            builder.RegisterModule(new TicketingModule(_configuration, _services));

            builder.RegisterModule(new CommandHandlingModule(_configuration));
            builder.RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory));
            builder.RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
