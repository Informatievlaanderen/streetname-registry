namespace StreetNameRegistry.Projections.Wms
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;

        public DbSet<StreetName.StreetNameHelper> StreetNameHelper { get; set; }
        public DbSet<StreetNameHelperV2.StreetNameHelperV2> StreetNameHelperV2 { get; set; }

        public DbSet<T> Get<T>() where T : class, new()
        {
            if (typeof(T) == typeof(StreetName.StreetNameHelper))
                return (StreetNameHelper as DbSet<T>)!;

            throw new NotImplementedException($"DbSet not found of type {typeof(T)}");
        }

        // This needs to be here to please EF
        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options)
        {
            Database.SetCommandTimeout(10 * 60);
        }
    }
}
