namespace StreetNameRegistry.Producer
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using global::Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class ProducerContextMigrationFactory : RunnerDbContextMigrationFactory<ProducerContext>
    {
        public ProducerContextMigrationFactory()
            : base("ProducerProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Producer,
                Table = MigrationTables.Producer
            };

        protected override ProducerContext CreateContext(DbContextOptions<ProducerContext> migrationContextOptions)
            => new ProducerContext(migrationContextOptions);
    }
}
