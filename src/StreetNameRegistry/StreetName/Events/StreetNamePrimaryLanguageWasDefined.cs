namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Crab)]
    [EventName("StreetNamePrimaryLanguageWasDefined")]
    [EventDescription("De primaire taal waarin de straatnaam beschikbaar is, werd bepaald.")]
    public sealed class StreetNamePrimaryLanguageWasDefined : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Primaire officiële taal van de straatnaam. Mogelijkheden: Dutch, French of German.")]
        public Language PrimaryLanguage { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNamePrimaryLanguageWasDefined(
            StreetNameId streetNameId,
            Language primaryLanguage)
        {
            StreetNameId = streetNameId;
            PrimaryLanguage = primaryLanguage;
        }

        [JsonConstructor]
        private StreetNamePrimaryLanguageWasDefined(
            Guid streetNameId,
            Language primaryLanguage,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                primaryLanguage) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
