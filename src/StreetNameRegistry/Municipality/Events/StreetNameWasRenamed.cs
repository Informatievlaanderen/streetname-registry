namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd hernoemd.")]
    public sealed class StreetNameWasRenamed : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameWasRenamed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is gekoppeld.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de doelStraatnaam.")]
        public int DestinationPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRenamed(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            PersistentLocalId destinationPersistentLocalId)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            DestinationPersistentLocalId = destinationPersistentLocalId;
        }

        [JsonConstructor]
        private StreetNameWasRenamed(
            Guid municipalityId,
            int persistentLocalId,
            int destinationPersistentLocalId,
            ProvenanceData provenance
        ) :
            this(
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalId(destinationPersistentLocalId))
            => SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.Add(DestinationPersistentLocalId.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
