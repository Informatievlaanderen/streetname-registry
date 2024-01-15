namespace StreetNameRegistry.Projections.Integration.Infrastructure
{
    using System;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;

    public class IntegrationModule : Module
    {
        public IntegrationModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<IntegrationModule>();
            var connectionString = configuration.GetConnectionString("IntegrationProjections");
            services.AddScoped<IEventsRepository>(_ => new EventsRepository(configuration.GetConnectionString("events")));

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnNpgSqlServer(services, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(IntegrationContext), Schema.Integration, MigrationTables.Integration);
        }

        private static void RunOnNpgSqlServer(
            IServiceCollection services,
            string connectionString)
        {
            services
                .AddNpgsql<IntegrationContext>(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Integration, Schema.Integration);
                });
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<IntegrationContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(IntegrationContext));
        }
    }
}
