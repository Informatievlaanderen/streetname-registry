namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNameWasMigrated")]
    [EventDescription("De straatnaam is gemigreerd naar de gemeente aggregate.")]
    public sealed class StreetNameWasMigrated : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Interne GUID van de gemeente naar waar de straatnaam is verplaatst.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasMigrated(StreetNameId streetNameId, MunicipalityId municipalityId, PersistentLocalId persistentLocalId)
        {
            StreetNameId = streetNameId;
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
        }

        [JsonConstructor]
        private StreetNameWasMigrated(
            Guid streetNameId,
            Guid municipalityId,
            int persistentLocalId,
            ProvenanceData provenance) :
            this(
                new StreetNameId(streetNameId),
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
