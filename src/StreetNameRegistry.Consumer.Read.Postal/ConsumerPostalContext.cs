namespace StreetNameRegistry.Consumer.Read.Postal
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using StreetNameRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class ConsumerPostalContext : RunnerDbContext<ConsumerPostalContext>
    {
        public DbSet<PostalConsumerItem> PostalConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerPostalContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerPostalContext(DbContextOptions<ConsumerPostalContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadPostal;
    }

    public class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerPostalContext>
    {
        public ConsumerPostalContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerPostalAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ConsumerPostalContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadPostal, Schema.ConsumerReadPostal);
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerPostalContext(builder.Options);
        }
    }
}
