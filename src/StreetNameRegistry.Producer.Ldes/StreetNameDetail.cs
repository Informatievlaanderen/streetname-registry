namespace StreetNameRegistry.Producer.Ldes
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Municipality;
    using NodaTime;
    using StreetNameRegistry.Infrastructure;

    public class StreetNameDetail
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int StreetNamePersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameGerman { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionEnglish { get; set; }
        public string? HomonymAdditionGerman { get; set; }

        public StreetNameStatus Status { get; set; }
        public bool IsRemoved { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }
        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class StreetNameDetailConfiguration : IEntityTypeConfiguration<StreetNameDetail>
    {
        private const string TableName = "StreetName";

        public void Configure(EntityTypeBuilder<StreetNameDetail> builder)
        {
            builder.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.StreetNamePersistentLocalId)
                .IsClustered();

            builder.Property(x => x.StreetNamePersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.NisCode);

            builder.Property(p => p.NameDutch);
            builder.Property(p => p.NameFrench);
            builder.Property(p => p.NameEnglish);
            builder.Property(p => p.NameGerman);

            builder.Property(p => p.HomonymAdditionDutch);
            builder.Property(p => p.HomonymAdditionFrench);
            builder.Property(p => p.HomonymAdditionEnglish);
            builder.Property(p => p.HomonymAdditionGerman);

            builder.Property(StreetNameDetail.VersionTimestampBackingPropertyName)
                .HasColumnName(nameof(StreetNameDetail.VersionTimestamp));
            builder.Ignore(p => p.VersionTimestamp);

            builder.HasIndex(x => x.NisCode);
        }
    }
}
