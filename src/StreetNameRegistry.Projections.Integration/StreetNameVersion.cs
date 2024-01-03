namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using StreetNameRegistry.Infrastructure;

    public sealed class StreetNameVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public long Position { get; set; }
        public int PersistentLocalId { get; set; }
        public string? Status { get; set; }
        public string? NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public bool IsRemoved { get; set; }

        public string PuriId { get; set; }
        public string Namespace { get; set; }
        public string VersionAsString { get; set; }
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public long IdempotenceKey { get; set; }

        public StreetNameVersion()
        {  }
    }

    public sealed class StreetNameVersionConfiguration : IEntityTypeConfiguration<StreetNameVersion>
    {
        internal const string TableName = "StreetNameVersions";

        public void Configure(EntityTypeBuilder<StreetNameVersion> builder)
        {
            builder.ToTable(TableName, Schema.Integration)
                .HasKey(x => x.Position)
                .IsClustered();
            builder.Property(x => x.PersistentLocalId).HasColumnName("position");
            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id");
            builder.Property(x => x.NisCode).HasColumnName("nis_code");
            builder.Property(x => x.Status).HasColumnName("status");

            builder.Property(x => x.NameDutch).HasColumnName("name_dutch");
            builder.Property(x => x.NameFrench).HasColumnName("name_french");
            builder.Property(x => x.NameGerman).HasColumnName("name_german");
            builder.Property(x => x.NameEnglish).HasColumnName("name_english");

            builder.Property(x => x.HomonymAdditionDutch).HasColumnName("homonym_addition_dutch");
            builder.Property(x => x.HomonymAdditionFrench).HasColumnName("homonym_addition_french");
            builder.Property(x => x.HomonymAdditionGerman).HasColumnName("homonym_addition_german");
            builder.Property(x => x.HomonymAdditionEnglish).HasColumnName("homonym_addition_english");

            builder.Property(x => x.IsRemoved).HasColumnName("is_removed");

            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(StreetNameVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(x => x.IdempotenceKey).HasColumnName("idempotence_key");

            builder.Ignore(x => x.VersionTimestamp);

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.PersistentLocalId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.NameDutch);
            builder.HasIndex(x => x.IsRemoved);
        }
    }
}
