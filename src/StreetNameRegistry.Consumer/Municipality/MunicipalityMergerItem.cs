namespace StreetNameRegistry.Consumer.Municipality
{
    using System;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class MunicipalityMergerItem
    {
        public Guid MunicipalityId { get; set; }
        public bool IsRetired { get; set; }
    }

    public class MunicipalityMergerItemConfiguration : IEntityTypeConfiguration<MunicipalityMergerItem>
    {
        private const string TableName = "MunicipalityMergers";

        public void Configure(EntityTypeBuilder<MunicipalityMergerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerProjections)
                .HasKey(x => x.MunicipalityId)
                .IsClustered(false);
        }
    }
}
