namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameHomonymAdditionWasCorrected")]
    [EventDescription("De homoniemtoevoeging van de straatnaam werd gecorrigeerd.")]
    public class StreetNameHomonymAdditionWasCorrected : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Homoniemtoevoeging aan de straatnaam.")]
        public string HomonymAddition { get; }

        [EventPropertyDescription("Taal (voluit, EN) waarin de officiële straatnaam staat.")]
        public Language? Language { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionWasCorrected(
            StreetNameId streetNameId,
            StreetNameHomonymAddition homonymAddition)
        {
            StreetNameId = streetNameId;
            HomonymAddition = homonymAddition.HomonymAddition;
            Language = homonymAddition.Language;
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionWasCorrected(
            Guid streetNameId,
            string homonymAddition,
            Language? language,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                new StreetNameHomonymAddition(homonymAddition, language)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
