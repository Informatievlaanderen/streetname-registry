namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetNameRegistry.Municipality;

    [EventTags(EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd voorgesteld.")]
    public sealed class StreetNameWasProposedForMunicipalityMerger : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameWasProposedForMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public string NisCode { get; }

        [EventPropertyDescription("De straatnamen in de officiële en (eventuele) faciliteitentaal van de gemeente. Mogelijkheden: Dutch, French, German of English.")]
        public IDictionary<Language, string> StreetNameNames { get; }

        [EventPropertyDescription("Homoniemtoevoeging aan de straatnaam.")]
        public IDictionary<Language, string> HomonymAdditions { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Lijst van interne GUIDs van de gefusioneerde gemeenten waarvoor deze straatnaam is voorgesteld.")]
        public List<Guid> MergedMunicipalityIds { get; }

        [EventPropertyDescription("Lijst van objectidentificatoren van de straatnamen van de gefusioneerde gemeenten waarvoor deze straatnaam is voorgesteld.")]
        public List<int> MergedStreetNamePersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasProposedForMunicipalityMerger(
            MunicipalityId municipalityId,
            NisCode nisCode,
            Names streetNameNames,
            HomonymAdditions homonymAdditions,
            PersistentLocalId persistentLocalId,
            List<MunicipalityId> mergedMunicipalityIds,
            List<PersistentLocalId> mergedStreetNamePersistentLocalIds)
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            StreetNameNames = streetNameNames.ToDictionary();
            HomonymAdditions = homonymAdditions.ToDictionary();
            PersistentLocalId = persistentLocalId;
            MergedMunicipalityIds = mergedMunicipalityIds.Select(x => (Guid)x).ToList();
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds.Select(x => (int)x).ToList();
        }

        [JsonConstructor]
        private StreetNameWasProposedForMunicipalityMerger(
            Guid municipalityId,
            string nisCode,
            IDictionary<Language, string> streetNameNames,
            IDictionary<Language, string> homonymAdditions,
            int persistentLocalId,
            List<Guid> mergedMunicipalityIds,
            List<int> mergedStreetNamePersistentLocalIds,
            ProvenanceData provenance) :
            this(
                new MunicipalityId(municipalityId),
                new NisCode(nisCode),
                new Names(streetNameNames),
                new HomonymAdditions(homonymAdditions),
                new PersistentLocalId(persistentLocalId),
                mergedMunicipalityIds.Select(x=> new MunicipalityId(x)).ToList(),
                mergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)).ToList())
        => SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(NisCode);
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(StreetNameNames.Select(streetNameName => $"{streetNameName.Key}: {streetNameName.Value}"));
            fields.AddRange(HomonymAdditions.Select(homonymAddition => $"{homonymAddition.Key}: {homonymAddition.Value}"));
            fields.AddRange(MergedMunicipalityIds.Select(mergedMunicipalityId => mergedMunicipalityId.ToString("D")));
            fields.AddRange(MergedStreetNamePersistentLocalIds.Select(mergedStreetNamePersistentLocalId => mergedStreetNamePersistentLocalId.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
