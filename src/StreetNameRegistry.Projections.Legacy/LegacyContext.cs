namespace StreetNameRegistry.Projections.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using StreetNameList;
    using StreetNameListV2;
    using StreetNameSyndication;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string StreetNameListViewCountName = "vw_StreetNameListIds";

        public DbSet<StreetNameListItem> StreetNameList { get; set; }
        public DbSet<StreetNameListItemV2> StreetNameListV2 { get; set; }
        public DbSet<StreetNameListMunicipality> StreetNameListMunicipality { get; set; }
        public DbSet<StreetNameDetail.StreetNameDetail> StreetNameDetail { get; set; }
        public DbSet<StreetNameDetailV2.StreetNameDetailV2> StreetNameDetailV2 { get; set; }
        public DbSet<StreetNameSyndicationItem> StreetNameSyndication { get; set; }

        public DbSet<StreetNameListViewCount> StreetNameListViewCount { get; set; }

        public DbSet<T> Get<T>() where T : class, new()
        {
            if (typeof(T) == typeof(StreetNameListItem))
                return (StreetNameList as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameListItemV2))
                return (StreetNameListV2 as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameListMunicipality))
                return (StreetNameListMunicipality as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameDetail.StreetNameDetail))
                return (StreetNameDetail as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameDetailV2.StreetNameDetailV2))
                return (StreetNameDetailV2 as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameSyndicationItem))
                return (StreetNameSyndication as DbSet<T>)!;

            if (typeof(T) == typeof(StreetNameListViewCount))
                return (StreetNameListViewCount as DbSet<T>)!;

            throw new NotImplementedException($"DbSet not found of type {typeof(T)}");
        }

        // This needs to be here to please EF
        public LegacyContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StreetNameListViewCount>()
                .HasNoKey()
                .ToView(StreetNameListViewCountName, Schema.Legacy);
        }
    }
}
