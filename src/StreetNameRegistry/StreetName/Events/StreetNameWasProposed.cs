namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameWasProposed")]
    [EventDescription("De straatnaam kreeg status 'voorgesteld'.")]
    public sealed class StreetNameWasProposed : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasProposed(StreetNameId streetNameId)
            => StreetNameId = streetNameId;

        [JsonConstructor]
        private StreetNameWasProposed(
            Guid streetNameId,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
