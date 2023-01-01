namespace StreetNameRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using global::Microsoft.EntityFrameworkCore;
    using Infrastructure;

    public sealed class WfsContextMigrationFactory : RunnerDbContextMigrationFactory<WfsContext>
    {
        public WfsContextMigrationFactory()
            : base("WfsProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Wfs,
                Table = MigrationTables.Wfs
            };

        protected override WfsContext CreateContext(DbContextOptions<WfsContext> migrationContextOptions)
            => new WfsContext(migrationContextOptions);
    }
}
