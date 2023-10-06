namespace StreetNameRegistry.Consumer.Read.Postal
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StreetNameRegistry.Infrastructure;

    public class PostalConsumerItem
    {
        public string PostalCode { get; set; }
        public string? NisCode { get; set; }
        public PostalStatus Status { get; set; }
        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        private PostalConsumerItem() { }

        public PostalConsumerItem(string postalCode)
        {
            PostalCode = postalCode;
            Status = PostalStatus.Realized;
        }
    }

    public struct PostalStatus
    {
        public static readonly PostalStatus Realized = new PostalStatus("Realized");
        public static readonly PostalStatus Retired = new PostalStatus("Retired");

        public string Status { get; }

        private PostalStatus(string status) => Status = status;

        public static PostalStatus Parse(string status)
        {
            if (status != Realized.Status &&
                status != Retired.Status)
            {
                throw new NotImplementedException($"Cannot parse {status} to PostalStatus");
            }

            return new PostalStatus(status);
        }

        public static implicit operator string(PostalStatus status) => status.Status;
    }

    public class PostalConsumerItemConfiguration : IEntityTypeConfiguration<PostalConsumerItem>
    {
        public const string TableName = "PostalItems";

        public void Configure(EntityTypeBuilder<PostalConsumerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadPostal)
                .HasKey(x => x.PostalCode)
                .IsClustered();

            builder.Property(x => x.NisCode);
            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder
                .Property(x => x.Status)
                .HasConversion(
                    postalStatus => postalStatus.Status,
                    status => PostalStatus.Parse(status));

            builder.HasIndex(x => x.NisCode);
        }
    }
}
