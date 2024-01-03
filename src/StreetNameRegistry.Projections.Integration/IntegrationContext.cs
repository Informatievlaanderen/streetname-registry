namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using StreetNameRegistry.Infrastructure;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<StreetNameLatestItem> StreetNameLatestItems => Set<StreetNameLatestItem>();
        public DbSet<StreetNameVersion> StreetNameVersions => Set<StreetNameVersion>();

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }

    public class ConfigBasedIntegrationContextFactory : IDesignTimeDbContextFactory<IntegrationContext>
    {
        public IntegrationContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "IntegrationProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddJsonFile($"appsettings.development.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<IntegrationContext>();

            var connectionString = configuration
                                       .GetConnectionString(migrationConnectionStringName)
                                   ?? throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");
            Console.WriteLine(connectionString);
            builder
                .UseNpgsql(connectionString, npgSqlOptions =>
                {
                    npgSqlOptions.EnableRetryOnFailure();
                    npgSqlOptions.MigrationsHistoryTable(
                        MigrationTables.Integration,
                        Schema.Integration);
                    //npgSqlOptions.UseNetTopologySuite();
                    npgSqlOptions.CommandTimeout(260);
                });

            return new IntegrationContext(builder.Options);
        }
    }
}
