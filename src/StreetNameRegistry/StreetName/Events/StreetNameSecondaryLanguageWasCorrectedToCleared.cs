namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Crab)]
    [EventName("StreetNameSecondaryLanguageWasCorrectedToCleared")]
    [EventDescription("De secundaire taalcode waarin de straatnaam beschikbaar is, werd gewist (via correctie).")]
    public sealed class StreetNameSecondaryLanguageWasCorrectedToCleared : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameSecondaryLanguageWasCorrectedToCleared(StreetNameId streetNameId)
            => StreetNameId = streetNameId;

        [JsonConstructor]
        private StreetNameSecondaryLanguageWasCorrectedToCleared(
            Guid streetNameId,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
