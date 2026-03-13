namespace StreetNameRegistry.Projections.Feed.StreetNameFeed
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class StreetNameDocument
    {
        public int PersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public StreetNameDocumentContent Document { get; set; }

        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }
        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set
            {
                LastChangedOnAsDateTimeOffset = value.ToBelgianDateTimeOffset();
                Document.VersionId = value.ToBelgianDateTimeOffset();
            }
        }

        private StreetNameDocument()
        {
            Document = new StreetNameDocumentContent();
            IsRemoved = false;
        }

        public StreetNameDocument(
            int persistentLocalId,
            string nisCode,
            Instant createdTimestamp,
            List<GeografischeNaam> names)
        {
            PersistentLocalId = persistentLocalId;
            RecordCreatedAt = createdTimestamp;

            Document = new StreetNameDocumentContent
            {
                PersistentLocalId = persistentLocalId,
                NisCode = nisCode,
                Names = names,
                HomonymAdditions = new List<GeografischeNaam>(),
                Status = StraatnaamStatus.Voorgesteld,
            };

            LastChangedOn = createdTimestamp;
        }
    }

    public sealed class StreetNameDocumentContent
    {
        public int PersistentLocalId { get; set; }
        public string NisCode { get; set; }
        public List<GeografischeNaam> Names { get; set; }
        public List<GeografischeNaam> HomonymAdditions { get; set; }
        public StraatnaamStatus? Status { get; set; }

        public DateTimeOffset VersionId { get; set; }

        public StreetNameDocumentContent()
        {
            Names = new List<GeografischeNaam>();
            HomonymAdditions = new List<GeografischeNaam>();
        }
    }

    public sealed class StreetNameDocumentConfiguration : IEntityTypeConfiguration<StreetNameDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public StreetNameDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<StreetNameDocument> b)
        {
            b.ToTable("StreetNameDocuments", Schema.Feed)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            b.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.LastChangedOnAsDateTimeOffset)
                .HasColumnName("LastChangedOn");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset)
                .HasColumnName("RecordCreatedAt");

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<StreetNameDocumentContent>(v, _serializerSettings)!);

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);
        }
    }
}
