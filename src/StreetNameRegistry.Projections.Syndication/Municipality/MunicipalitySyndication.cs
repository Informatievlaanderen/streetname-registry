namespace StreetNameRegistry.Projections.Syndication.Municipality
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;

    public sealed class MunicipalitySyndicationItem
    {
        public Guid MunicipalityId { get; set; }
        public string? NisCode { get; set; }
        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }
        public string? Version { get; set; }
        public long Position { get; set; }
    }

    public sealed class MunicipalityItemConfiguration : IEntityTypeConfiguration<MunicipalitySyndicationItem>
    {
        private const string TableName = "MunicipalitySyndication";

        public void Configure(EntityTypeBuilder<MunicipalitySyndicationItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => new { x.MunicipalityId, x.Position })
                .IsClustered(false);

            builder.Property(x => x.NisCode);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.Version);
            builder.HasIndex(x => x.NisCode).IsClustered();
        }
    }
}
