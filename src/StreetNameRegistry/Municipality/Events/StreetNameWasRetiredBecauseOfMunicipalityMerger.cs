namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetNameRegistry.Municipality;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd gehistoreerd in functie van een gemeentefusie.")]
    public sealed class StreetNameWasRetiredBecauseOfMunicipalityMerger : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameWasRetiredBecauseOfMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is gekoppeld.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van de nieuwe straatnamen.")]
        public List<int> NewPersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasRetiredBecauseOfMunicipalityMerger(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            IEnumerable<PersistentLocalId> newPersistentLocalIds)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            NewPersistentLocalIds = newPersistentLocalIds.Select(x => (int)x).ToList();
        }

        [JsonConstructor]
        private StreetNameWasRetiredBecauseOfMunicipalityMerger(
            Guid municipalityId,
            int persistentLocalId,
            List<int> newPersistentLocalIds,
            ProvenanceData provenance
        ) :
            this(
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId),
                newPersistentLocalIds.Select(x => new PersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(NewPersistentLocalIds.Select(x => x.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
