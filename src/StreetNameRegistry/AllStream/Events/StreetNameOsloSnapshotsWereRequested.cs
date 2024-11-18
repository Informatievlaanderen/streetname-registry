namespace StreetNameRegistry.AllStream.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Newtonsoft.Json;

    [HideEvent]
    [EventName(EventName)]
    [EventDescription("Nieuwe OSLO snapshots werd aangevraagd voor de straatnamen.")]
    public sealed class StreetNameOsloSnapshotsWereRequested : IHasProvenance, ISetProvenance, IMessage
    {
        public const string EventName = "StreetNameOsloSnapshotsWereRequested"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificatoren van de straatnamen.")]
        public IEnumerable<int> PersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameOsloSnapshotsWereRequested(
            IEnumerable<PersistentLocalId> persistentLocalIds)
        {
            PersistentLocalIds = persistentLocalIds
                .Select(x => (int)x)
                .ToList();
        }

        [JsonConstructor]
        private StreetNameOsloSnapshotsWereRequested(
            IEnumerable<int> persistentLocalIds,
            ProvenanceData provenance)
            : this(
                persistentLocalIds.Select(x => new PersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.AddRange(PersistentLocalIds.Select(x => x.ToString()));

            return fields;
        }
    }
}
