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
    [EventDescription("De homoniemtoevoegingen van de straatnaam (in een bepaalde taal) werd(en) verwijderd.")]
    public sealed class StreetNameHomonymAdditionsWereRemoved : IMunicipalityEvent
    {
        public const string EventName = "StreetNameHomonymAdditionsWereRemoved"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is gekoppeld.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("De homoniemtoevoeging voor deze talen werd verwijderd.")]
        public List<Language> Languages { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionsWereRemoved(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            List<Language> languages)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            Languages = languages;
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionsWereRemoved(
            Guid municipalityId,
            int persistentLocalId,
            List<Language> languages,
            ProvenanceData provenance) :
            this(
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId),
                languages)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(Languages.Select(language => language.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
