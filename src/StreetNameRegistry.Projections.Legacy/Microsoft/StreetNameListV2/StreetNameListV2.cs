namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameListV2
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using Infrastructure;
    using Municipality;

    public sealed class StreetNameListItemV2
    {
        public static readonly string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? NameDutchSearch { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglishSearch { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool Removed { get; set; }

        public Language? PrimaryLanguage { get; set; }

        public string NisCode { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public sealed class StreetNameListV2Configuration : IEntityTypeConfiguration<StreetNameListItemV2>
    {
        internal const string TableName = "StreetNameListV2";

        public void Configure(EntityTypeBuilder<StreetNameListItemV2> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            builder.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(StreetNameListItemV2.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.Property(x => x.MunicipalityId);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglishSearch);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Status);

            builder.Property(x => x.Removed);

            builder.Property(x => x.PrimaryLanguage);

            builder.Property(x => x.NisCode);

            builder.HasIndex(x => x.NisCode);
            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameGermanSearch);
            builder.HasIndex(x => x.NameEnglishSearch);

            builder.HasIndex(x => x.MunicipalityId);
            builder.HasIndex(x => x.Removed);

        }
    }
}
