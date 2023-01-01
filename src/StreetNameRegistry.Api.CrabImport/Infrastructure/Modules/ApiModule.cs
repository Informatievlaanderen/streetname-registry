namespace StreetNameRegistry.Api.CrabImport.Infrastructure.Modules
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CrabImport;
    using CrabImport;
    using StreetNameRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure.Modules;
    using IdempotencyAutofac = Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using IdempotencyMicrosoft = Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency.Microsoft;
    using TracingAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using TracingMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
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
                .RegisterModule(new TracingAutofac.DataDogModule(_configuration))

                .RegisterModule(new IdempotencyAutofac.IdempotencyModule(
                    _services,
                    _configuration.GetSection(IdempotencyAutofac.IdempotencyConfiguration.Section).Get<IdempotencyAutofac.IdempotencyConfiguration>().ConnectionString,
                    new IdempotencyAutofac.IdempotencyMigrationsTableInfo(Schema.Import),
                    new IdempotencyAutofac.IdempotencyTableInfo(Schema.Import),
                    _loggerFactory))

                .RegisterModule(new EventHandlingAutofac.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))

                .RegisterModule(new ProjectionHandlingAutofac.EnvelopeModule())

                .RegisterModule(new CommandHandlingModule(_configuration))

                .RegisterModule(new CrabImportModule(
                    _configuration.GetConnectionString("CrabImport"),
                    Schema.Import,
                    _loggerFactory));

            builder
                .RegisterType<IdempotentCommandHandlerModule>()
                .AsSelf();

            builder
                .RegisterType<IdempotentCommandHandlerModuleProcessor>()
                .As<IIdempotentCommandHandlerModuleProcessor>();

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        public void Load(IServiceCollection services)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            services
                .RegisterModule(new TracingMicrosoft.DataDogModule(_configuration))

                .RegisterModule(new IdempotencyMicrosoft.IdempotencyModule(
                    _configuration.GetSection(IdempotencyMicrosoft.IdempotencyConfiguration.Section).Get<IdempotencyMicrosoft.IdempotencyConfiguration>().ConnectionString,
                    new IdempotencyMicrosoft.IdempotencyMigrationsTableInfo(Schema.Import),
                    new IdempotencyMicrosoft.IdempotencyTableInfo(Schema.Import),
                    _loggerFactory))

                .RegisterModule(new EventHandlingMicrosoft.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))

                .RegisterModule(new ProjectionHandlingMicrosoft.EnvelopeModule())

                .RegisterModule(new CommandHandlingModule(_configuration))

                .RegisterModule(new CrabImportModule(
                    _configuration.GetConnectionString("CrabImport"),
                    Schema.Import,
                    _loggerFactory));

            services
                .AddTransient<IdempotentCommandHandlerModule>()
                .AddTransient<IIdempotentCommandHandlerModuleProcessor, IdempotentCommandHandlerModuleProcessor>()
                .AddTransient<ProblemDetailsHelper>();
        }
    }
}
