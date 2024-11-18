namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("De NisCode van de gemeente werd gewijzigd.")]
    public sealed class MunicipalityNisCodeWasChanged: IMunicipalityEvent
    {
        public const string EventName = "MunicipalityNisCodeWasChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente.")]
        public Guid MunicipalityId { get; }
        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente.")]
        public string NisCode { get; }

        [EventPropertyDescription("De lijst van straatnaam objectidentificatoren die geïmpacteerd zijn.")]
        public List<int> StreetNamePersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityNisCodeWasChanged(
            MunicipalityId municipalityId,
            NisCode nisCode,
            List<PersistentLocalId> streetNamePersistentLocalIds)
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            StreetNamePersistentLocalIds = streetNamePersistentLocalIds.Select(id => (int)id).ToList();
        }

        [JsonConstructor]
        private MunicipalityNisCodeWasChanged(
            Guid municipalityId,
            string nisCode,
            List<int> streetNamePersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new MunicipalityId(municipalityId),
                new NisCode(nisCode),
                streetNamePersistentLocalIds.Select(id => new PersistentLocalId(id)).ToList())
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(NisCode);
            fields.AddRange(StreetNamePersistentLocalIds.Select(id => id.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
