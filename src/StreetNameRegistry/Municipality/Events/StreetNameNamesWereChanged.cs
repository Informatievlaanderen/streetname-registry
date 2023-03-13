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
    [EventDescription("De na(a)m(en) van de straatnaam (in een bepaalde taal) werd(en) gewijzigd.")]
    public sealed class StreetNameNamesWereChanged : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameNamesWereChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is gekoppeld.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("De straatnamen in de officiële en (eventuele) faciliteitentaal van de gemeente. Mogelijkheden: Dutch, French, German of English.")]
        public IDictionary<Language, string> StreetNameNames { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameNamesWereChanged(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            Names streetNameNames)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            StreetNameNames = streetNameNames.ToDictionary();
        }

        [JsonConstructor]
        private StreetNameNamesWereChanged(
            Guid municipalityId,
            int persistentLocalId,
            IDictionary<Language, string> streetNameNames,
            ProvenanceData provenance) :
            this(
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId),
                new Names(streetNameNames))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(StreetNameNames.Select(streetNameName => $"{streetNameName.Key}: {streetNameName.Value}"));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
