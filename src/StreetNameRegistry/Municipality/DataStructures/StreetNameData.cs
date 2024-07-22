namespace StreetNameRegistry.Municipality.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    public sealed class StreetNameData
    {
        public int StreetNamePersistentLocalId { get; }
        public StreetNameStatus Status { get; }

        public IDictionary<Language, string> Names { get; }
        public IDictionary<Language, string> HomonymAdditions { get; }

        public bool IsRemoved { get; }
        public bool IsRenamed { get; }

        public Guid? LegacyStreetNameId { get; }

        public List<int> MergedStreetNamePersistentLocalIds { get; }
        public StreetNameStatus? MergedStatus { get; }

        public string LastEventHash { get; }

        public ProvenanceData LastProvenanceData { get; }

        public StreetNameData(MunicipalityStreetName streetName)
            : this(streetName.PersistentLocalId,
                streetName.Status,
                streetName.Names,
                streetName.HomonymAdditions,
                streetName.IsRemoved,
                streetName.IsRenamed,
                streetName.LegacyStreetNameId,
                streetName.MergedStreetNamePersistentLocalIds.ToList(),
                streetName.MergedStatus,
                streetName.LastEventHash,
                streetName.LastProvenanceData)
        { }

        public StreetNameData(
            PersistentLocalId streetNamePersistentLocalId,
            StreetNameStatus status,
            Names names,
            HomonymAdditions homonymAdditions,
            bool isRemoved,
            bool isRenamed,
            StreetNameId? legacyStreetNameId,
            List<PersistentLocalId> mergedStreetNamePersistentLocalIds,
            StreetNameStatus? mergedStatus,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Status = status;
            Names = names.ToDictionary();
            HomonymAdditions = homonymAdditions.ToDictionary();
            IsRemoved = isRemoved;
            IsRenamed = isRenamed;
            LegacyStreetNameId = legacyStreetNameId is null ? (Guid?)null : legacyStreetNameId;
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds.Select(x => (int)x).ToList();
            MergedStatus = mergedStatus;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }

        [JsonConstructor]
        private StreetNameData(
            int streetNamePersistentLocalId,
            StreetNameStatus status,
            IDictionary<Language, string> names,
            IDictionary<Language, string> homonymAdditions,
            bool isRemoved,
            bool? isRenamed,
            Guid? legacyStreetNameId,
            List<int>? mergedStreetNamePersistentLocalIds,
            StreetNameStatus? mergedStatus,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Status = status;
            Names = names;
            HomonymAdditions = homonymAdditions;
            IsRemoved = isRemoved;
            IsRenamed = isRenamed ?? false;
            LegacyStreetNameId = legacyStreetNameId;
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds ?? [];
            MergedStatus = mergedStatus;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }
    }
}
