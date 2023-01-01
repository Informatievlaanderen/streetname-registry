namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Projections.Legacy;
    using Projections.Syndication;
    using DataDogAutofac = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using DataDogMicrosoft = Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;

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
            builder
                .RegisterModule(new DataDogAutofac.DataDogModule(_configuration))
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        public void Load(IServiceCollection services)
        {
            services
                .RegisterModule(new DataDogMicrosoft.DataDogModule(_configuration))
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory));

            services.AddTransient<ProblemDetailsHelper>();
        }
    }
}
