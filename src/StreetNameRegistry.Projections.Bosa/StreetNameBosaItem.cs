namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Municipality;
    using NodaTime;
    using StreetNameRegistry.Infrastructure;

    /// <summary>
    /// The full current state of a street name and the identity of its current Bosa version.
    /// </summary>
    public sealed class StreetNameBosaItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnBackingPropertyName = nameof(CreatedOnAsDateTimeOffset);
        public const string VersionIdBackingPropertyName = nameof(VersionIdAsDateTimeOffset);
        public const string BeginLifeSpanBackingPropertyName = nameof(BeginLifeSpanAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string? NisCode { get; set; }
        public StreetNameStatus Status { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public bool IsRemoved { get; set; }

        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Time of the last change, of any kind.
        /// </summary>
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }
        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        private DateTimeOffset CreatedOnAsDateTimeOffset { get; set; }
        public Instant CreatedOn
        {
            get => Instant.FromDateTimeOffset(CreatedOnAsDateTimeOffset);
            set => CreatedOnAsDateTimeOffset = value.ToDateTimeOffset();
        }

        /// <summary>
        /// The versionIdentifier of the current version, every change except an end or removal changes it.
        /// </summary>
        private DateTimeOffset VersionIdAsDateTimeOffset { get; set; }
        public Instant VersionId
        {
            get => Instant.FromDateTimeOffset(VersionIdAsDateTimeOffset);
            set => VersionIdAsDateTimeOffset = value.ToDateTimeOffset();
        }

        /// <summary>
        /// The start of the current version: its beginLifeSpanVersion and the validFrom in its status.
        /// </summary>
        private DateTimeOffset BeginLifeSpanAsDateTimeOffset { get; set; }
        public Instant BeginLifeSpan
        {
            get => Instant.FromDateTimeOffset(BeginLifeSpanAsDateTimeOffset);
            set => BeginLifeSpanAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public StreetNameBosaItem()
        { }
    }

    public sealed class StreetNameBosaItemConfiguration : IEntityTypeConfiguration<StreetNameBosaItem>
    {
        internal const string TableName = "streetnames";

        public void Configure(EntityTypeBuilder<StreetNameBosaItem> builder)
        {
            builder.ToTable(TableName, Schema.Bosa)
                .HasKey(x => x.PersistentLocalId);
            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id").ValueGeneratedNever();

            builder.Property(x => x.MunicipalityId).HasColumnName("municipality_id");
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

            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(StreetNameBosaItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(StreetNameBosaItem.CreatedOnBackingPropertyName).HasColumnName("created_on");
            builder.Property(StreetNameBosaItem.VersionIdBackingPropertyName).HasColumnName("version_id");
            builder.Property(StreetNameBosaItem.BeginLifeSpanBackingPropertyName).HasColumnName("begin_life_span");

            builder.Ignore(x => x.VersionTimestamp);
            builder.Ignore(x => x.CreatedOn);
            builder.Ignore(x => x.VersionId);
            builder.Ignore(x => x.BeginLifeSpan);

            builder.HasIndex(x => x.MunicipalityId);
        }
    }
}
