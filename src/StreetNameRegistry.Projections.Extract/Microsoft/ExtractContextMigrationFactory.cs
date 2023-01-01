namespace StreetNameRegistry.Projections.Extract.Microsoft
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft;
    using global::Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

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
