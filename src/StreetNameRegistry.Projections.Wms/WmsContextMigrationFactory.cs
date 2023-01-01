namespace StreetNameRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using global::Microsoft.EntityFrameworkCore;
    using Infrastructure;

    public sealed class WmsContextMigrationFactory : RunnerDbContextMigrationFactory<WmsContext>
    {
        public WmsContextMigrationFactory()
            : base("WmsProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Wms,
                Table = MigrationTables.Wms
            };

        protected override WmsContext CreateContext(DbContextOptions<WmsContext> migrationContextOptions)
            => new WmsContext(migrationContextOptions);
    }
}
