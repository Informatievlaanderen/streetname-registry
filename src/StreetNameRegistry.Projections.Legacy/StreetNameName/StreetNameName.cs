namespace StreetNameRegistry.Projections.Legacy.StreetNameName
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using NodaTime;
    using StreetName;

    public sealed class StreetNameName
    {
        public static readonly string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid StreetNameId { get; set; }
        public int PersistentLocalId { get; set; }

        public string? NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameDutchSearch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGerman { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameEnglishSearch { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool IsFlemishRegion { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public string? GetNameValueByLanguage(Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return NameDutch;

                case Language.French:
                    return NameFrench;

                case Language.German:
                    return NameGerman;

                case Language.English:
                    return NameEnglish;
            }
        }
    }

    public sealed class StreetNameNameConfiguration : IEntityTypeConfiguration<StreetNameName>
    {
        private const string TableName = "StreetNameName";

        public void Configure(EntityTypeBuilder<StreetNameName> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.StreetNameId)
                .IsClustered(false);

            builder.Property(p => p.NisCode);
            builder.Property(p => p.PersistentLocalId);
            builder.Property(p => p.NameDutch);
            builder.Property(p => p.NameDutchSearch);
            builder.Property(p => p.NameFrench);
            builder.Property(p => p.NameFrenchSearch);
            builder.Property(p => p.NameGerman);
            builder.Property(p => p.NameGermanSearch);
            builder.Property(p => p.NameEnglish);
            builder.Property(p => p.NameEnglishSearch);
            builder.Property(p => p.Status);
            builder.Property(p => p.IsFlemishRegion);

            builder.Property(StreetNameName.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(p => p.VersionTimestamp);

            builder.Property(p => p.Complete);
            builder.Property(p => p.Removed);

            // This index speeds up the hardcoded first filter in StreetNameBosaQuery
            builder.HasIndex(p => new { p.Removed, p.IsFlemishRegion, p.Complete });

            builder.HasIndex(p => p.NisCode);
            builder.HasIndex(p => p.PersistentLocalId).IsClustered();

            builder.HasIndex(StreetNameName.VersionTimestampBackingPropertyName);
            builder.HasIndex(p => p.Status);

            builder.HasIndex(p => p.NameDutch);
            builder.HasIndex(p => p.NameFrench);
            builder.HasIndex(p => p.NameGerman);
            builder.HasIndex(p => p.NameEnglish);

            builder.HasIndex(p => p.NameDutchSearch);
            builder.HasIndex(p => p.NameFrenchSearch);
            builder.HasIndex(p => p.NameGermanSearch);
            builder.HasIndex(p => p.NameEnglishSearch);
        }
    }
}
