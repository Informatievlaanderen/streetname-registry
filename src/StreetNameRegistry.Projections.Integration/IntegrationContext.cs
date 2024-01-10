namespace StreetNameRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
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
}
