namespace StreetNameRegistry.Projections.Legacy.StreetNameSyndication
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft.MigrationExtensions;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure;
    using Municipality;
    using NodaTime;

    public sealed class StreetNameSyndicationItem
    {
        public long Position { get; set; }

        public Guid? MunicipalityId { get; set; }
        public Guid? StreetNameId { get; set; }
        public int? PersistentLocalId { get; set; }
        public string? NisCode { get; set; }
        public string? ChangeType { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }
        public string? NameEnglish { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }
        public string? HomonymAdditionEnglish { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool IsComplete { get; set; }

        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }
        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set => LastChangedOnAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string? EventDataAsXml { get; set; }
        public DateTimeOffset SyndicationItemCreatedAt { get; set; }

        public StreetNameSyndicationItem CloneAndApplyEventInfo(
            long newPosition,
            string eventName,
            Instant lastChangedOn,
            Action<StreetNameSyndicationItem> editFunc)
        {
            var newItem = new StreetNameSyndicationItem
            {
                Position = newPosition,
                ChangeType = eventName,

                StreetNameId = StreetNameId,
                NisCode = NisCode,

                PersistentLocalId = PersistentLocalId,
                MunicipalityId = MunicipalityId,

                NameDutch = NameDutch,
                NameEnglish = NameEnglish,
                NameFrench = NameFrench,
                NameGerman = NameGerman,

                HomonymAdditionDutch = HomonymAdditionDutch,
                HomonymAdditionEnglish = HomonymAdditionEnglish,
                HomonymAdditionFrench = HomonymAdditionFrench,
                HomonymAdditionGerman = HomonymAdditionGerman,

                Status = Status,
                IsComplete = IsComplete,

                Reason = Reason,
                Modification = Modification,
                Operator = Operator,
                Organisation = Organisation,
                Application = Application,

                RecordCreatedAt = RecordCreatedAt,
                LastChangedOn = lastChangedOn,
                SyndicationItemCreatedAt = DateTimeOffset.UtcNow
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public sealed class StreetNameSyndicationItemConfiguration : IEntityTypeConfiguration<StreetNameSyndicationItem>
    {
        private const string TableName = "StreetNameSyndication";

        public void Configure(EntityTypeBuilder<StreetNameSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.Position)
                .IsClustered();

            b.Property(x => x.Position).ValueGeneratedNever();
            b.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            b.Property(x => x.StreetNameId);
            b.Property(x => x.MunicipalityId);
            b.Property(x => x.NisCode);
            b.Property(x => x.ChangeType);

            b.Property(x => x.NameDutch);
            b.Property(x => x.NameFrench);
            b.Property(x => x.NameGerman);
            b.Property(x => x.NameEnglish);

            b.Property(x => x.HomonymAdditionDutch);
            b.Property(x => x.HomonymAdditionFrench);
            b.Property(x => x.HomonymAdditionGerman);
            b.Property(x => x.HomonymAdditionEnglish);

            b.Property(x => x.Status);
            b.Property(x => x.IsComplete);

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");
            b.Property(x => x.LastChangedOnAsDateTimeOffset).HasColumnName("LastChangedOn");

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);
            b.Property(x => x.EventDataAsXml);
            b.Property(x => x.SyndicationItemCreatedAt).IsRequired();

            b.Ignore(x => x.RecordCreatedAt);
            b.Ignore(x => x.LastChangedOn);

            b.HasIndex(x => x.StreetNameId);
            b.HasIndex(x => x.PersistentLocalId);
        }
    }
}
