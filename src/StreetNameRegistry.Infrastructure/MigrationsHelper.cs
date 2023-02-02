namespace StreetNameRegistry.Infrastructure
{
    using System;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static void Run(
            string sequencesConnectionString,
            ILoggerFactory? loggerFactory = null)
        {
            var logger = loggerFactory?.CreateLogger<MigrationsLogger>();

            Policy
                .Handle<SqlException>()
                .WaitAndRetry(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5); //NOSONAR Random is safe here
                        logger?.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .Execute(() =>
                    {
                        logger?.LogInformation("Running EF Migrations.");
                        RunInternal(sequencesConnectionString, loggerFactory);
                    });
        }

        private static void RunInternal(string sequencesConnectionString, ILoggerFactory? loggerFactory)
        {
            var migratorOptions = new DbContextOptionsBuilder<SequenceContext>()
                .UseSqlServer(
                    sequencesConnectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence);
                    });

            if (loggerFactory is not null)
            {
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);
            }

            using var migrator = new SequenceContext(migratorOptions.Options);
            migrator.Database.Migrate();
        }
    }
}
