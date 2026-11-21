namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    using System;
    using Amazon.S3;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using StreetNameRegistry.Infrastructure;

    public class BosaModule : Module
    {
        public BosaModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<BosaModule>();
            var connectionString = configuration.GetConnectionString("BosaProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnNpgSqlServer(services, connectionString!);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            RegisterDailyFiles(configuration, services, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(BosaContext), Schema.Bosa, MigrationTables.Bosa);
        }

        private static void RegisterDailyFiles(
            IConfiguration configuration,
            IServiceCollection services,
            ILogger logger)
        {
            services.AddSingleton<BosaDay>(sp => new BosaDay(sp.GetRequiredService<IOptions<BosaOptions>>().Value));
            services.AddSingleton<IBosaXmlWriter, BosaXmlWriter>();
            services.AddSingleton<IBosaDailyFilePublisher, BosaDailyFilePublisher>();

            var bucket = configuration[$"Bosa:{nameof(BosaOptions.Bucket)}"];
            if (string.IsNullOrWhiteSpace(bucket))
            {
                services.AddSingleton<IBosaFileStore, InMemoryBosaFileStore>();
                logger.LogWarning("No bucket configured, keeping Bosa files in memory!");
            }
            else
            {
                services.AddSingleton<IBosaFileStore>(_ => new S3BosaFileStore(new AmazonS3Client(), bucket));
            }
        }

        private static void RunOnNpgSqlServer(
            IServiceCollection services,
            string connectionString)
        {
            services
                .AddNpgsql<BosaContext>(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Bosa, Schema.Bosa);
                });
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<BosaContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(BosaContext));
        }
    }
}
