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
        public string LastEventHash { get; }

        public List<Guid> MergedMunicipalityIds { get; }
        public List<int> MergedStreetNamePersistentLocalIds { get; }

        public ProvenanceData LastProvenanceData { get; }

        public StreetNameData(MunicipalityStreetName streetName)
            : this(streetName.PersistentLocalId,
                streetName.Status,
                streetName.Names,
                streetName.HomonymAdditions,
                streetName.IsRemoved,
                streetName.IsRenamed,
                streetName.LegacyStreetNameId,
                streetName.MergedMunicipalityIds.ToList(),
                streetName.MergedStreetNamePersistentLocalIds.ToList(),
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
            List<MunicipalityId> mergedMunicipalityIds,
            List<PersistentLocalId> mergedStreetNamePersistentLocalIds,
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
            MergedMunicipalityIds = mergedMunicipalityIds.Select(x => (Guid)x).ToList();
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds.Select(x => (int)x).ToList();
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
            List<Guid>? mergedMunicipalityIds,
            List<int>? mergedStreetNamePersistentLocalIds,
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
            MergedMunicipalityIds = mergedMunicipalityIds ?? [];
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds ?? [];
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }
    }
}
