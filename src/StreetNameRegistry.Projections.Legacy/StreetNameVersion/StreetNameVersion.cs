namespace StreetNameRegistry.Projections.Legacy.StreetNameVersion
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public class StreetNameVersion
    {
        public int? PersistentLocalId { get; set; }
        public Guid StreetNameId { get; set; }

        public string NisCode { get; set; }

        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }

        public string HomonymAdditionDutch { get; set; }
        public string HomonymAdditionFrench { get; set; }
        public string HomonymAdditionGerman { get; set; }
        public string HomonymAdditionEnglish { get; set; }

        public StreetNameStatus? Status { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        public long Position { get; set; }

        public Instant? VersionTimestamp
        {
            get => VersionTimestampAsDateTimeOffset == null ? (Instant?)null : Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset.Value);
            set => VersionTimestampAsDateTimeOffset = value?.ToDateTimeOffset();
        }

        public DateTimeOffset? VersionTimestampAsDateTimeOffset { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string Reason { get; set; }

        public StreetNameVersion CloneAndApplyEventInfo(
            long newPosition,
            Action<StreetNameVersion> editFunc)
        {
            var newItem = new StreetNameVersion
            {
                Position = newPosition,

                PersistentLocalId = PersistentLocalId,
                StreetNameId = StreetNameId,

                NisCode = NisCode,

                NameDutch = NameDutch,
                NameEnglish = NameEnglish,
                NameFrench = NameFrench,
                NameGerman = NameGerman,

                HomonymAdditionDutch = HomonymAdditionDutch,
                HomonymAdditionEnglish = HomonymAdditionEnglish,
                HomonymAdditionFrench = HomonymAdditionFrench,
                HomonymAdditionGerman = HomonymAdditionGerman,

                Status = Status,

                Complete = Complete,
                Removed = Removed,

                VersionTimestamp = VersionTimestamp,

                Application = Application,
                Modification = Modification,
                Operator = Operator,
                Organisation = Organisation,
                Reason = Reason
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public class StreetNameVersionConfiguration : IEntityTypeConfiguration<StreetNameVersion>
    {
        private const string TableName = "StreetNameVersions";

        public void Configure(EntityTypeBuilder<StreetNameVersion> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(x => new { x.StreetNameId, x.Position })
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.PersistentLocalId);
            builder.Property(x => x.NisCode);

            builder.Property(x => x.NameDutch);
            builder.Property(x => x.NameFrench);
            builder.Property(x => x.NameGerman);
            builder.Property(x => x.NameEnglish);

            builder.Property(x => x.HomonymAdditionDutch);
            builder.Property(x => x.HomonymAdditionFrench);
            builder.Property(x => x.HomonymAdditionGerman);
            builder.Property(x => x.HomonymAdditionEnglish);

            builder.Property(x => x.Status);
            builder.Property(x => x.Complete);
            builder.Property(x => x.Removed);

            builder.Property(x => x.VersionTimestampAsDateTimeOffset).HasColumnName("VersionTimestamp");

            builder.Property(x => x.Application);
            builder.Property(x => x.Modification);
            builder.Property(x => x.Operator);
            builder.Property(x => x.Organisation);
            builder.Property(x => x.Reason);

            builder.Ignore(x => x.VersionTimestamp);

            builder.HasIndex(x => x.PersistentLocalId).ForSqlServerIsClustered();
            builder.HasIndex(x => x.Removed);
        }
    }
}
