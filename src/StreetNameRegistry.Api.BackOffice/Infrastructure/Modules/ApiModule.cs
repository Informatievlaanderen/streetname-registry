namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Consumer;
    using Handlers.Sqs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;
    using IdemPotencyAutofac = Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using IdemPotencyMicrosoft = Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency.Microsoft;
    using DataDogAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using DataDogMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using EventHandlingAutofac = Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using EventHandlingMicrosoft = Be.Vlaanderen.Basisregisters.EventHandling.Microsoft;
    using StreamStoreAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using StreamStoreMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Microsoft;

    public sealed class ApiModule : Module, IServiceCollectionModule
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

            builder
                .RegisterModule(new DataDogAutofac.DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder.RegisterModule(new IdemPotencyAutofac.IdempotencyModule(
                _services,
                _configuration.GetSection(IdemPotencyAutofac.IdempotencyConfiguration.Section).Get<IdemPotencyAutofac.IdempotencyConfiguration>()
                    .ConnectionString,
                new IdemPotencyAutofac.IdempotencyMigrationsTableInfo(Schema.Import),
                new IdemPotencyAutofac.IdempotencyTableInfo(Schema.Import),
                _loggerFactory));

            builder.RegisterModule(new EventHandlingAutofac.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly,
                eventSerializerSettings));

            builder.RegisterModule(new StreamStoreAutofac.EnvelopeModule());
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

        public void Load(IServiceCollection services)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            services.RegisterModule(new DataDogMicrosoft.DataDogModule(_configuration));

            services.AddTransient<ProblemDetailsHelper>();

            services.AddTransient<IfMatchHeaderValidator>();

            services.RegisterModule(new IdemPotencyMicrosoft.IdempotencyModule(
                _configuration.GetSection(IdemPotencyMicrosoft.IdempotencyConfiguration.Section).Get<IdemPotencyMicrosoft.IdempotencyConfiguration>()
                    .ConnectionString,
                new IdemPotencyMicrosoft.IdempotencyMigrationsTableInfo(Schema.Import),
                new IdemPotencyMicrosoft.IdempotencyTableInfo(Schema.Import),
                _loggerFactory));

            services.RegisterModule(new EventHandlingMicrosoft.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly,
                eventSerializerSettings));

            services.RegisterModule(new StreamStoreMicrosoft.EnvelopeModule());
            services.RegisterModule(new SequenceModule(_configuration, _services, _loggerFactory));
            services.RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory));
            services.RegisterModule(new MediatRModule());
            services.RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]));
            services.RegisterModule(new TicketingModule(_configuration, _services));

            services.RegisterModule(new CommandHandlingModule(_configuration));
            services.RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory));
            services.RegisterSnapshotModule(_configuration);
        }
    }
}
