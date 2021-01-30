namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameSecondaryLanguageWasCorrected")]
    [EventDescription("De secundaire taal waarin de straatnaam beschikbaar is, werd gecorrigeerd.")]
    public class StreetNameSecondaryLanguageWasCorrected : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Secundaire officiële taal (voluit, EN) van de straatnaam.")]
        public Language SecondaryLanguage { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameSecondaryLanguageWasCorrected(
            StreetNameId streetNameId,
            Language secondaryLanguage)
        {
            StreetNameId = streetNameId;
            SecondaryLanguage = secondaryLanguage;
        }

        [JsonConstructor]
        private StreetNameSecondaryLanguageWasCorrected(
            Guid streetNameId,
            Language secondaryLanguage,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                secondaryLanguage) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
