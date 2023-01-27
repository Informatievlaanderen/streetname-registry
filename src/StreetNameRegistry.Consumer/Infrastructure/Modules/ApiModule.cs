namespace StreetNameRegistry.Consumer.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections;
    using StreetNameRegistry.Infrastructure;
    using StreetNameRegistry.Infrastructure.Modules;

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
            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new CommandHandlingModule(_configuration))
                .RegisterModule(new ProjectorModule(_configuration))
                .RegisterSnapshotModule(_configuration);

            builder
                .RegisterProjectionMigrator<ConsumerContextFactory>(_configuration, _loggerFactory)
                .RegisterProjections<MunicipalityConsumerProjection, ConsumerContext>(
                    context => new MunicipalityConsumerProjection(),
                    ConnectedProjectionSettings.Default);

            builder.Populate(_services);
        }
    }
}
