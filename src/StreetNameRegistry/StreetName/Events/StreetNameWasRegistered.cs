namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameWasRegistered")]
    [EventDescription("De straatnaam werd aangemaakt in het register.")]
    public class StreetNameWasRegistered : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public string NisCode { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRegistered(
            StreetNameId streetNameId,
            MunicipalityId municipalityId,
            NisCode nisCode)
        {
            StreetNameId = streetNameId;
            MunicipalityId = municipalityId;
            NisCode = nisCode;
        }

        [JsonConstructor]
        private StreetNameWasRegistered(
            Guid streetNameId,
            Guid municipalityId,
            string nisCode,
            ProvenanceData provenance) :
            this(
                  new StreetNameId(streetNameId),
                  new MunicipalityId(municipalityId),
                  new NisCode(nisCode)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
