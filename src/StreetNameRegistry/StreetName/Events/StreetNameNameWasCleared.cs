namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameNameWasCleared")]
    [EventDescription("De naam van de straatnaam (in een bepaalde taal) werd gewist.")]
    public class StreetNameNameWasCleared : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarvoor de officiële naam gewist werd.")]
        public Language? Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameNameWasCleared(
            StreetNameId streetNameId,
            Language? language)
        {
            StreetNameId = streetNameId;
            Language = language;
        }

        [JsonConstructor]
        private StreetNameNameWasCleared(
            Guid streetNameId,
            Language? language,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                language) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
