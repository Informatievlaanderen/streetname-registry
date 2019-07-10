namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("StreetNamePersistentLocalIdentifierWasAssigned")]
    [EventDescription("De straatnaam kreeg een persistente lokale identificator toegekend.")]
    public class StreetNamePersistentLocalIdWasAssigned : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        public Guid StreetNameId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }
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
