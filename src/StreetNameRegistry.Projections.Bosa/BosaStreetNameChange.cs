namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Municipality;
    using StreetNameRegistry.Infrastructure;

    /// <summary>
    /// What happened to a street name within a day, from which its entries in that day's add and update files are built.
    /// </summary>
    public sealed class BosaStreetNameChange
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public DateOnly Day { get; set; }
        public int PersistentLocalId { get; set; }
        public string Data { get; set; } = string.Empty;
        public long Position { get; set; }

        public BosaStreetNameChange()
        { }

        public StreetNameDayChange GetData()
            => string.IsNullOrEmpty(Data)
                ? new StreetNameDayChange()
                : JsonSerializer.Deserialize<StreetNameDayChange>(Data, JsonOptions)!;

        public void SetData(StreetNameDayChange data) => Data = JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// A street name within a day has at most two versions for Bosa:
    /// the version it had at the start of the day (an update), and one new version (an add),
    /// in which all new versions of that day are collapsed.
    /// </summary>
    public sealed class StreetNameDayChange
    {
        /// <summary>
        /// The version at the start of the day, with its state when it was ended.
        /// Empty when the street name was created that day.
        /// </summary>
        public StreetNameVersionPart? Previous { get; set; }

        /// <summary>
        /// The new version of that day, with its latest state.
        /// </summary>
        public StreetNameVersionPart? Current { get; set; }
    }

    public sealed class StreetNameVersionPart
    {
        public StreetNameVersionState State { get; set; } = new StreetNameVersionState();
        public DateTimeOffset? EndLifeSpan { get; set; }

        /// <summary>
        /// Other street names this version was created from (predecessors) or replaced by (successors), by a municipality merger.
        /// </summary>
        public List<int> LinkedPersistentLocalIds { get; set; } = new List<int>();
        public List<StreetNameVersionEvent> Events { get; set; } = new List<StreetNameVersionEvent>();
    }

    public sealed class StreetNameVersionState
    {
        public int PersistentLocalId { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public string? NisCode { get; set; }
        public StreetNameStatus Status { get; set; }

        public string? NameDutch { get; set; }
        public string? NameFrench { get; set; }
        public string? NameGerman { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }

        public DateTimeOffset VersionId { get; set; }
        public DateTimeOffset BeginLifeSpan { get; set; }
        public DateTimeOffset ValidFrom { get; set; }

        public static StreetNameVersionState From(StreetNameBosaItem item) => new StreetNameVersionState
        {
            PersistentLocalId = item.PersistentLocalId,
            Namespace = item.Namespace,
            NisCode = item.NisCode,
            Status = item.Status,
            NameDutch = item.NameDutch,
            NameFrench = item.NameFrench,
            NameGerman = item.NameGerman,
            HomonymAdditionDutch = item.HomonymAdditionDutch,
            HomonymAdditionFrench = item.HomonymAdditionFrench,
            HomonymAdditionGerman = item.HomonymAdditionGerman,
            VersionId = item.VersionId.ToDateTimeOffset(),
            BeginLifeSpan = item.BeginLifeSpan.ToDateTimeOffset(),
            ValidFrom = item.ValidFrom.ToDateTimeOffset()
        };
    }

    public sealed class StreetNameVersionEvent
    {
        public DateTimeOffset Date { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public sealed class BosaStreetNameChangeConfiguration : IEntityTypeConfiguration<BosaStreetNameChange>
    {
        internal const string TableName = "streetname_changes";

        public void Configure(EntityTypeBuilder<BosaStreetNameChange> builder)
        {
            builder.ToTable(TableName, Schema.Bosa)
                .HasKey(x => new { x.Day, x.PersistentLocalId });

            builder.Property(x => x.Day).HasColumnName("day");
            builder.Property(x => x.PersistentLocalId).HasColumnName("persistent_local_id");
            builder.Property(x => x.Data).HasColumnName("data").HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.Position).HasColumnName("position");
        }
    }
}
