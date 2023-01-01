namespace StreetNameRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using global::Microsoft.EntityFrameworkCore;
    using Infrastructure;

    public sealed class ExtractContextMigrationFactory : RunnerDbContextMigrationFactory<ExtractContext>
    {
        public ExtractContextMigrationFactory() :
            base("ExtractProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Extract,
                Table = MigrationTables.Extract
            };

        protected override ExtractContext CreateContext(DbContextOptions<ExtractContext> migrationContextOptions)
            => new ExtractContext(migrationContextOptions);
    }
}
