namespace StreetNameRegistry.Projections.Legacy.StreetNameNameV2
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using Municipality;
    using NodaTime;

    public sealed class StreetNameNameV2
    {
        public static readonly string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }

        public Guid MunicipalityId { get; set; }

        public string NisCode { get; set; }

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

    public sealed class StreetNameNameV2Configuration : IEntityTypeConfiguration<StreetNameNameV2>
    {
        private const string TableName = "StreetNameNameV2";

        public void Configure(EntityTypeBuilder<StreetNameNameV2> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            builder.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(p => p.MunicipalityId);
            builder.Property(p => p.NisCode);
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

            builder.Property(StreetNameNameV2.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(p => p.VersionTimestamp);

            builder.Property(p => p.Removed);

            // This index speeds up the hardcoded first filter in StreetNameBosaQuery
            builder.HasIndex(p => new { p.Removed, p.IsFlemishRegion});

            builder.HasIndex(p => p.NisCode);

            builder.HasIndex(StreetNameNameV2.VersionTimestampBackingPropertyName);
            builder.HasIndex(p => p.Status);

            builder.HasIndex(p => p.NameDutch);
            builder.HasIndex(p => p.NameFrench);
            builder.HasIndex(p => p.NameGerman);
            builder.HasIndex(p => p.NameEnglish);

            builder.HasIndex(p => p.NameDutchSearch);
            builder.HasIndex(p => p.NameFrenchSearch);
            builder.HasIndex(p => p.NameGermanSearch);
            builder.HasIndex(p => p.NameEnglishSearch);

            builder.HasIndex(p => p.MunicipalityId);
        }
    }
}
