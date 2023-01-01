namespace StreetNameRegistry.Projections.Wfs.Microsoft
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft;
    using global::Microsoft.EntityFrameworkCore;
    using Infrastructure;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;

        public DbSet<StreetNameHelper.StreetNameHelper> StreetNameHelper { get; set; }
        public DbSet<StreetNameHelperV2.StreetNameHelperV2> StreetNameHelperV2 { get; set; }

        public DbSet<T> Get<T>() where T : class, new()
        {
            if (typeof(T) == typeof(StreetName.StreetNameHelper))
                return (StreetNameHelper as DbSet<T>)!;

            throw new NotImplementedException($"DbSet not found of type {typeof(T)}");
        }

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
