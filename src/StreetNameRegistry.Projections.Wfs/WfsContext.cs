namespace StreetNameRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;

        public DbSet<StreetNameHelperV2.StreetNameHelperV2> StreetNameHelperV2 => Set<StreetNameHelperV2.StreetNameHelperV2>();

        // This needs to be here to please EF
        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options)
        {
            Database.SetCommandTimeout(10 * 60);
        }
    }
}
