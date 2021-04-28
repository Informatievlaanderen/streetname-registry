namespace StreetNameRegistry.Projections.Legacy.StreetNameList
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class StreetNameListItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int? PersistentLocalId { get; set; }
        public Guid StreetNameId { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        public Language? PrimaryLanguage { get; set; }

        public string? NisCode { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class StreetNameListConfiguration : IEntityTypeConfiguration<StreetNameListItem>
    {
        internal const string TableName = "StreetNameList";

        public void Configure(EntityTypeBuilder<StreetNameListItem> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.StreetNameId)
                .IsClustered(false);

            builder.Property(x => x.PersistentLocalId);

            builder.Property(StreetNameListItem.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Status);

            builder.Property(x => x.Complete);
            builder.Property(x => x.Removed);

            builder.Property(x => x.PrimaryLanguage);

            builder.Property(x => x.NisCode);

            builder.HasIndex(x => x.PersistentLocalId).IsClustered();
            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.Status);

            // This index speeds up the hardcoded first filter in StreetNameListQuery
            builder.HasIndex(x => new { x.Complete, x.Removed });
            builder.HasIndex(x => new { x.Complete, x.Removed, x.PersistentLocalId });
        }
    }
}
