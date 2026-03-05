namespace StreetNameRegistry.Projections.Feed.StreetNameFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StreetNameRegistry.Infrastructure;

    public class StreetNameFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public int PersistentLocalId { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private StreetNameFeedItem() { }

        public StreetNameFeedItem(long position, int page, int persistentLocalId) : this()
        {
            PersistentLocalId = persistentLocalId;
            Page = page;
            Position = position;
        }
    }

    public class StreetNameFeedConfiguration : IEntityTypeConfiguration<StreetNameFeedItem>
    {
        private const string TableName = "StreetNameFeed";

        public void Configure(EntityTypeBuilder<StreetNameFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo("StreetNameFeedSequence", Schema.Feed);

            b.Property(x => x.CloudEventAsString)
                .HasColumnName("CloudEvent")
                .IsRequired();

            b.Property(x => x.PersistentLocalId).IsRequired();

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
            b.HasIndex(x => x.PersistentLocalId);
        }
    }
}
