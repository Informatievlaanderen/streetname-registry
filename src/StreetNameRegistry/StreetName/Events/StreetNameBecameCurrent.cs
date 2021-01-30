namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameBecameCurrent")]
    [EventDescription("De straatnaam kreeg status 'in gebruik'.")]
    public class StreetNameBecameCurrent : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameBecameCurrent(StreetNameId streetNameId)
            => StreetNameId = streetNameId;

        [JsonConstructor]
        private StreetNameBecameCurrent(
            Guid streetNameId,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
