namespace StreetNameRegistry.Projections.Feed
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using StreetNameFeed;
    using StreetNameRegistry.Infrastructure;

    public class FeedContext : RunnerDbContext<FeedContext>
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public override string ProjectionStateSchema => Schema.Feed;

        public DbSet<StreetNameFeedItem> StreetNameFeed { get; set; }

        public DbSet<StreetNameDocument> StreetNameDocuments { get; set; }

        // This needs to be here to please EF
        public FeedContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FeedContext(DbContextOptions<FeedContext> options, JsonSerializerSettings jsonSerializerSettings)
            : base(options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("StreetNameFeedSequence", Schema.Feed)
                .StartsAt(1)
                .IncrementsBy(1)
                .IsCyclic(false);

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new StreetNameFeedConfiguration());
            modelBuilder.ApplyConfiguration(new StreetNameDocumentConfiguration(_jsonSerializerSettings));
        }
    }
}
