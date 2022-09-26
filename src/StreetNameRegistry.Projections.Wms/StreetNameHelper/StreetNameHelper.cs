namespace StreetNameRegistry.Projections.Wms.StreetName
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using StreetNameRegistry.StreetName;

    public sealed class StreetNameHelper
    {
        public Guid StreetNameId { get; set; }

        public int? PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }

        public string? NisCode { get; set; }

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


        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

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

    public sealed class StreetNameConfiguration : IEntityTypeConfiguration<StreetNameHelper>
    {
        public const string TableName = "StreetNameHelper";

        public void Configure(EntityTypeBuilder<StreetNameHelper> builder)
        {
            builder.ToTable(TableName, Schema.Wms)
                .HasKey(x => x.StreetNameId)
                .IsClustered(false);

            builder.Property(x => x.PersistentLocalId);
            builder.Property(x => x.MunicipalityId);

            builder.Property(StreetNameHelper.VersionTimestampBackingPropertyName)
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
            builder.Property(x => x.Complete);
            builder.Property(x => x.Removed);

            builder.HasIndex(x => new { x.Removed, x.Complete});
            builder.HasIndex(x => x.MunicipalityId);
        }
    }
}
