namespace StreetNameRegistry.Projections.Integration.Merger
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StreetNameRegistry.Infrastructure;

    public sealed class StreetNameMergerItem
    {
        public int NewPersistentLocalId { get; set; }
        public int MergedPersistentLocalId { get; set; }

        public StreetNameMergerItem()
        {  }

        public StreetNameMergerItem(int newPersistentLocalId, int mergedPersistentLocalId)
        {
            NewPersistentLocalId = newPersistentLocalId;
            MergedPersistentLocalId = mergedPersistentLocalId;
        }
    }

    public sealed class StreetNameMergerItemConfiguration : IEntityTypeConfiguration<StreetNameMergerItem>
    {
        internal const string TableName = "streetname_merger_items";

        public void Configure(EntityTypeBuilder<StreetNameMergerItem> builder)
        {
            builder.ToTable(TableName, Schema.Integration)
                .HasKey(x => new { x.NewPersistentLocalId, x.MergedPersistentLocalId})
                .IsClustered();

            builder.Property(x => x.NewPersistentLocalId).HasColumnName("new_persistent_local_id");
            builder.Property(x => x.MergedPersistentLocalId).HasColumnName("merged_persistent_local_id");

            builder.HasIndex(x => x.NewPersistentLocalId);
            builder.HasIndex(x => x.MergedPersistentLocalId);
        }
    }
}
