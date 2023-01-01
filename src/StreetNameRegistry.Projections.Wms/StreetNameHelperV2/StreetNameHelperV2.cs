namespace StreetNameRegistry.Projections.Wms.StreetNameHelperV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using NodaTime;
    using Municipality;

    public sealed class StreetNameHelperV2
    {
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

        public StreetNameStatus Status { get; set; }
        public bool Removed { get; set; }


        public static readonly string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }
        public string? VersionAsString { get; protected set; }
    }

    public sealed class StreetNameConfiguration : IEntityTypeConfiguration<StreetNameHelperV2>
    {
        public const string TableName = "StreetNameHelperV2";

        public void Configure(EntityTypeBuilder<StreetNameHelperV2> builder)
        {
            builder.ToTable(TableName, Schema.Wms)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            builder.Property(x=> x.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(x => x.MunicipalityId);

            builder.Property(StreetNameHelperV2.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Property(p => p.VersionAsString);
            builder.Ignore(x => x.Version);

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
            builder.Property(x => x.Removed);

            builder.HasIndex(x => x.Removed);
            builder.HasIndex(x => x.MunicipalityId);
        }
    }
}
