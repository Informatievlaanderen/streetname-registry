namespace StreetNameRegistry.Projections.Bosa
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Npgsql;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class BosaContextMigrationFactory : NpgsqlRunnerDbContextMigrationFactory<BosaContext>
    {
        public BosaContextMigrationFactory()
            : base("BosaProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Bosa,
                Table = MigrationTables.Bosa
            };

        protected override BosaContext CreateContext(DbContextOptions<BosaContext> migrationContextOptions)
            => new BosaContext(migrationContextOptions);
    }
}
