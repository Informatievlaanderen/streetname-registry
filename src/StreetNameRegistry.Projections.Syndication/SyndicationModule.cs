namespace StreetNameRegistry.Projections.Syndication
{
    using System;
    using System.Net.Http;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Http;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft.MigrationExtensions;
    using global::Microsoft.Data.SqlClient;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Infrastructure;
    using Polly;
    using SyndicationAutofac = Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using SyndicationMicrosoft = Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication.Microsoft;

    public sealed class SyndicationModule : Module, IServiceCollectionModule
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public SyndicationModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<SyndicationContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<SyndicationContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<SyndicationContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<SyndicationContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(SyndicationContext));
        }

        private static void RegisterHttpClientAutofac(
            IConfiguration configuration,
            IServiceCollection services)
        {
            services
                .AddHttpClient(
                    SyndicationAutofac.RegistryAtomFeedReader.HttpClientName,
                    client => { client.DefaultRequestHeaders.Add("Accept", "application/atom+xml"); })
                .ConfigurePrimaryHttpMessageHandler(c => new TraceHttpMessageHandler(
                    new HttpClientHandler(),
                    configuration["DataDog:ServiceName"]))
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
                    .WaitAndRetryAsync(
                        5,
                        retryAttempt =>
                        {
                            var value = Math.Pow(2, retryAttempt) / 4;
                            var randomValue = new Random().Next((int) value * 3, (int) value * 5);
                            return TimeSpan.FromSeconds(randomValue);
                        }));
        }

        private static void RegisterHttpClient(
            IConfiguration configuration,
            IServiceCollection services)
        {
            services
                .AddHttpClient(
                    SyndicationMicrosoft.RegistryAtomFeedReader.HttpClientName,
                    client => { client.DefaultRequestHeaders.Add("Accept", "application/atom+xml"); })
                .ConfigurePrimaryHttpMessageHandler(c => new TraceHttpMessageHandler(
                    new HttpClientHandler(),
                    configuration["DataDog:ServiceName"]))
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
                    .WaitAndRetryAsync(
                        5,
                        retryAttempt =>
                        {
                            var value = Math.Pow(2, retryAttempt) / 4;
                            var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                            return TimeSpan.FromSeconds(randomValue);
                        }));
        }

        protected override void Load(ContainerBuilder builder)
        {
            var logger = _loggerFactory.CreateLogger<SyndicationModule>();
            var connectionString = _configuration.GetConnectionString("SyndicationProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(_configuration, _services, _loggerFactory, connectionString);
            else
                RunInMemoryDb(_services, _loggerFactory, logger);

            RegisterHttpClientAutofac(_configuration, _services);

            builder
                .RegisterType<SyndicationAutofac.RegistryAtomFeedReader>()
                .As<SyndicationAutofac.IRegistryAtomFeedReader>();

            builder
                .RegisterType<SyndicationAutofac.FeedProjector<SyndicationContext>>()
                .AsSelf();
        }

        public void Load(IServiceCollection services)
        {
            var logger = _loggerFactory.CreateLogger<SyndicationModule>();
            var connectionString = _configuration.GetConnectionString("SyndicationProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(_configuration, services, _loggerFactory, connectionString);
            else
                RunInMemoryDb(services, _loggerFactory, logger);

            RegisterHttpClient(_configuration, services);

            services
                .AddTransient<SyndicationMicrosoft.IRegistryAtomFeedReader, SyndicationMicrosoft.RegistryAtomFeedReader>()
                .AddTransient<SyndicationMicrosoft.FeedProjector<Microsoft.SyndicationContext>>();
        }
    }
}
