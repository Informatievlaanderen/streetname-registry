namespace StreetNameRegistry.Projections.Legacy.StreetNameDetailV2
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using Municipality;
    using NodaTime;

    public sealed class StreetNameDetailV2
    {
        public static readonly string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool Removed { get; set; }

        public string? LastEventHash { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public sealed class StreetNameDetailV2Configuration : IEntityTypeConfiguration<StreetNameDetailV2>
    {
        internal const string TableName = "StreetNameDetailsV2";

        public void Configure(EntityTypeBuilder<StreetNameDetailV2> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            builder.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(StreetNameDetailV2.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.VersionTimestamp);
            builder.Property(x => x.MunicipalityId);
            builder.Property(x => x.NisCode);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Status);
            builder.Property(x => x.LastEventHash);
            builder.Property(x => x.Removed);

            builder.HasIndex(x => x.Removed);
            builder.HasIndex(x => x.MunicipalityId);
        }
    }
}
