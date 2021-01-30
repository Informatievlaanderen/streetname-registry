namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNamePrimaryLanguageWasCorrected")]
    [EventDescription("De primaire taal waarin de straatnaam beschikbaar is, werd gecorrigeerd.")]
    public class StreetNamePrimaryLanguageWasCorrected : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Primaire officiële taal (voluit, EN) van de straatnaam.")]
        public Language PrimaryLanguage { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNamePrimaryLanguageWasCorrected(
            StreetNameId streetNameId,
            Language primaryLanguage)
        {
            StreetNameId = streetNameId;
            PrimaryLanguage = primaryLanguage;
        }

        [JsonConstructor]
        private StreetNamePrimaryLanguageWasCorrected(
            Guid streetNameId,
            Language primaryLanguage,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                primaryLanguage) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
