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

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd hernoemd.")]
    public sealed class StreetNameWasRenamedV2 : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameWasRenamed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public string NisCode { get; }

        [EventPropertyDescription("De straatnamen in de officiële en (eventuele) faciliteitentaal van de gemeente. Mogelijkheden: Dutch, French, German of English.")]
        public IDictionary<Language, string> StreetNameNames { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRenamedV2(
            MunicipalityId municipalityId,
            NisCode nisCode,
            Names streetNameNames,
            PersistentLocalId persistentLocalId)
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            StreetNameNames = streetNameNames.ToDictionary();
            PersistentLocalId = persistentLocalId;
        }

        [JsonConstructor]
        private StreetNameWasRenamedV2(
            Guid municipalityId,
            string nisCode,
            IDictionary<Language, string> streetNameNames,
            int persistentLocalId,
            ProvenanceData provenance
        ) :
            this(
                new MunicipalityId(municipalityId),
                new NisCode(nisCode),
                new Names(streetNameNames),
                new PersistentLocalId(persistentLocalId))
        => SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(NisCode);
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(StreetNameNames.Select(streetNameName => $"{streetNameName.Key}: {streetNameName.Value}"));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
