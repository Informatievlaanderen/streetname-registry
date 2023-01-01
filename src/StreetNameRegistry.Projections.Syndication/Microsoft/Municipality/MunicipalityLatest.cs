namespace StreetNameRegistry.Projections.Syndication.Microsoft.Municipality
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;

    public sealed class MunicipalityLatestItem
    {
        public Guid MunicipalityId { get; set; }
        public string? NisCode { get; set; }

        public string? NameDutch { get; set; }
        public string? NameDutchSearch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameFrenchSearch { get; set; }
        public string? NameGerman { get; set; }
        public string? NameGermanSearch { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameEnglishSearch { get; set; }

        public Taal? PrimaryLanguage { get; set; }
        public string? Version { get; set; }

        public long Position { get; set; }
    }

    public sealed class MunicipalityLatestItemConfiguration : IEntityTypeConfiguration<MunicipalityLatestItem>
    {
        private const string TableName = "MunicipalityLatestSyndication";

        public void Configure(EntityTypeBuilder<MunicipalityLatestItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.MunicipalityId)
                .IsClustered(false);

            builder.Property(x => x.NisCode);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameDutchSearch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameFrenchSearch);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameGermanSearch);
            builder.Property(x => x.NameEnglish);
            builder.Property(x => x.NameEnglishSearch);
            builder.Property(x => x.PrimaryLanguage);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.NisCode).IsClustered();

            builder.HasIndex(x => x.NameDutchSearch);
            builder.HasIndex(x => x.NameFrenchSearch);
            builder.HasIndex(x => x.NameGermanSearch);
            builder.HasIndex(x => x.NameEnglishSearch);
        }
    }
}
