namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNamePrimaryLanguageWasCorrectedToCleared")]
    [EventDescription("De primare taalcode waarin de straatnaam beschikbaar is, werd gewist (via correctie).")]
    public class StreetNamePrimaryLanguageWasCorrectedToCleared : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNamePrimaryLanguageWasCorrectedToCleared(StreetNameId streetNameId)
            => StreetNameId = streetNameId;

        [JsonConstructor]
        private StreetNamePrimaryLanguageWasCorrectedToCleared(
            Guid streetNameId,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
