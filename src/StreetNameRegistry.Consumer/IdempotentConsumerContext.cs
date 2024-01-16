namespace StreetNameRegistry.Consumer
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using StreetNameRegistry.Infrastructure;

    public class IdempotentConsumerContext : SqlServerConsumerDbContext<IdempotentConsumerContext>
    {
        // This needs to be here to please EF
        public IdempotentConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IdempotentConsumerContext(DbContextOptions<IdempotentConsumerContext> options)
            : base(options)
        { }

        public override string ProcessedMessagesSchema => Schema.Consumer;
    }

    public sealed class IdempotentConsumerContextFactory : IDesignTimeDbContextFactory<IdempotentConsumerContext>
    {
        public IdempotentConsumerContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<IdempotentConsumerContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Consumer, Schema.Consumer);
                })
                .UseExtendedSqlServerMigrations();

            return new IdempotentConsumerContext(builder.Options);
        }
    }
}
