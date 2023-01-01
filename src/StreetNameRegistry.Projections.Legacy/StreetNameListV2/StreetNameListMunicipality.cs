namespace StreetNameRegistry.Projections.Legacy.StreetNameListV2
{
    using System;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using Municipality;

    public sealed class StreetNameListMunicipality
    {
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }
        public Language? PrimaryLanguage { get; set; }
        public Language? SecondaryLanguage { get; set; }
    }

    public sealed class StreetNameListMunicipalityConfiguration : IEntityTypeConfiguration<StreetNameListMunicipality>
    {
        internal const string TableName = "StreetNameListMunicipality";

        public void Configure(EntityTypeBuilder<StreetNameListMunicipality> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.MunicipalityId)
                .IsClustered();

            builder.Property(x => x.PrimaryLanguage);
            builder.Property(x => x.SecondaryLanguage);

            builder.Property(x => x.NisCode);
        }
    }
}
