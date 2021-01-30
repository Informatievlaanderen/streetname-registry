namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameNameWasCorrected")]
    [EventDescription("De naam van de straatnaam (in een bepaalde taal) werd gecorrigeerd.")]
    public class StreetNameNameWasCorrected : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Officiële spelling van de straatnaam.")]
        public string Name { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarin de officiële naam staat.")]
        public Language? Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameNameWasCorrected(
            StreetNameId streetNameId,
            StreetNameName name)
        {
            StreetNameId = streetNameId;
            Name = name.Name;
            Language = name.Language;
        }

        [JsonConstructor]
        private StreetNameNameWasCorrected(
            Guid streetNameId,
            string name,
            Language? language,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                new StreetNameName(name, language)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
