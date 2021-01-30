namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameHomonymAdditionWasCorrectedToCleared")]
    [EventDescription("De homoniemtoevoeging van de straatnaam werd gewist (via correctie).")]
    public class StreetNameHomonymAdditionWasCorrectedToCleared : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarin de officiële straatnaam staat.")]
        public Language? Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionWasCorrectedToCleared(
            StreetNameId streetNameId,
            Language? language)
        {
            StreetNameId = streetNameId;
            Language = language;
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionWasCorrectedToCleared(
            Guid streetNameId,
            Language? language,
            ProvenanceData provenance) :
            this(
                  new StreetNameId(streetNameId),
                  language) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
