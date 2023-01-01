namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;

    public sealed class StreetNameExtractItem
    {
        public Guid? StreetNameId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public bool Complete { get; set; }
        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameEnglish { get; set; }
        public string? NameGerman { get; set; }
        public string? NameUnknown { get; set; }
        public string? HomonymDutch { get; set; }
        public string? HomonymFrench { get; set; }
        public string? HomonymEnglish { get; set; }
        public string? HomonymGerman { get; set; }
        public string? HomonymUnknown { get; set; }
        public byte[]? DbaseRecord { get; set; }
    }

    public sealed class StreetNameExtractItemConfiguration : IEntityTypeConfiguration<StreetNameExtractItem>
    {
        private const string TableName = "StreetName";

        public void Configure(EntityTypeBuilder<StreetNameExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.StreetNameId)
                .IsClustered(false);

            builder.Property(p => p.StreetNamePersistentLocalId);
            builder.Property(p => p.Complete);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.NameDutch);
            builder.Property(p => p.NameFrench);
            builder.Property(p => p.NameEnglish);
            builder.Property(p => p.NameGerman);
            builder.Property(p => p.NameUnknown);
            builder.Property(p => p.HomonymDutch);
            builder.Property(p => p.HomonymFrench);
            builder.Property(p => p.HomonymEnglish);
            builder.Property(p => p.HomonymGerman);
            builder.Property(p => p.HomonymUnknown);

            builder.HasIndex(p => p.StreetNamePersistentLocalId).IsClustered();
        }
    }
}
