namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventTags(EventTag.For.Sync)]
    [EventName("StreetNamePersistentLocalIdentifierWasAssigned")]
    [EventDescription("De straatnaam kreeg een persistente lokale identificator toegekend.")]
    public class StreetNamePersistentLocalIdWasAssigned : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Tijdstip waarop de objectidentificator van de straatnaam werd toegekend.")]
        public Instant AssignmentDate { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNamePersistentLocalIdWasAssigned(
            StreetNameId streetNameId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            StreetNameId = streetNameId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private StreetNamePersistentLocalIdWasAssigned(
            Guid streetNameId,
            int persistentLocalId,
            Instant assignmentDate,
            ProvenanceData provenance) :
            this(new StreetNameId(streetNameId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
