namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("StreetNameOsloIdWasAssigned")]
    [EventDescription("De straatnaam kreeg een Oslo Id toegekend.")]
    public class StreetNameOsloIdWasAssigned : IHasStreetNameId, IHasProvenance, ISetProvenance
    {
        public Guid StreetNameId { get; }
        public int OsloId { get; }
        public Instant AssignmentDate { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameOsloIdWasAssigned(
            StreetNameId streetNameId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            StreetNameId = streetNameId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private StreetNameOsloIdWasAssigned(
            Guid streetNameId,
            int osloId,
            Instant assignmentDate,
            ProvenanceData provenance) :
            this(new StreetNameId(streetNameId),
                new OsloId(osloId),
                new OsloAssignmentDate(assignmentDate))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
