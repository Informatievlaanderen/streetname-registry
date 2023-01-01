namespace StreetNameRegistry.Projections.Extract
{
    using System;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft.MigrationExtensions;
    using Infrastructure;
    using global::Microsoft.Data.SqlClient;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;

    public sealed class ExtractModule : Module, IServiceCollectionModule
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly bool _enableRetry;

        public ExtractModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            bool enableRetry = true)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
            _enableRetry = enableRetry;
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString,
            bool enableRetry)
        {
            services
                .AddScoped(s => new TraceDbConnection<ExtractContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<ExtractContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<ExtractContext>>(), sqlServerOptions =>
                    {
                        if (enableRetry)
                            sqlServerOptions.EnableRetryOnFailure();

                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ExtractContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ExtractContext));
        }

        protected override void Load(ContainerBuilder builder)
        {
            var logger = _loggerFactory.CreateLogger<ExtractModule>();
            var connectionString = _configuration.GetConnectionString("ExtractProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(_configuration, _services, _loggerFactory, connectionString, _enableRetry);
            else
                RunInMemoryDb(_services, _loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ExtractContext), Schema.Extract, MigrationTables.Extract);
        }

        public void Load(IServiceCollection services)
        {
            var logger = _loggerFactory.CreateLogger<ExtractModule>();
            var connectionString = _configuration.GetConnectionString("ExtractProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(_configuration, services, _loggerFactory, connectionString, _enableRetry);
            else
                RunInMemoryDb(services, _loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ExtractContext), Schema.Extract, MigrationTables.Extract);
        }
    }
}
