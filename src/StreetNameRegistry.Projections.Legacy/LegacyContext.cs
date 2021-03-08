namespace StreetNameRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using StreetNameList;
    using StreetNameSyndication;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string StreetNameListViewCountName = "vw_StreetNameListIds";

        public DbSet<StreetNameListItem> StreetNameList { get; set; }
        public DbSet<StreetNameDetail.StreetNameDetail> StreetNameDetail { get; set; }
        public DbSet<StreetNameName.StreetNameName> StreetNameNames { get; set; }
        public DbSet<StreetNameSyndicationItem> StreetNameSyndication { get; set; }

        public DbSet<StreetNameListViewCount> StreetNameListViewCount { get; set; }

        // This needs to be here to please EF
        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StreetNameListViewCount>()
                .HasNoKey()
                .ToView(StreetNameListViewCountName, Schema.Legacy);
        }
    }
}
