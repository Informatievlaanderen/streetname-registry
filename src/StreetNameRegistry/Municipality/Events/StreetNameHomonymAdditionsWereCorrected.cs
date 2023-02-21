namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetNameRegistry.Municipality;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("De homoniemtoevoegingen van de straatnaam (in een bepaalde taal) werd(en) gecorrigeerd.")]
    public sealed class StreetNameHomonymAdditionsWereCorrected : IMunicipalityEvent
    {
        public const string EventName = "StreetNameHomonymAdditionsWereCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is gekoppeld.")]
        public Guid MunicipalityId { get; }

        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("De homoniemtoevoegingen in de officiële en (eventuele) faciliteitentaal van de gemeente. Mogelijkheden: Dutch, French, German of English.")]
        public IDictionary<Language, string> HomonymAdditions { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameHomonymAdditionsWereCorrected(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            List<StreetNameHomonymAddition> homonymAdditions)
        {
            MunicipalityId = municipalityId;
            PersistentLocalId = persistentLocalId;
            HomonymAdditions = homonymAdditions.ToDictionary(x => x.Language, y => y.HomonymAddition);
        }

        [JsonConstructor]
        private StreetNameHomonymAdditionsWereCorrected(
            Guid municipalityId,
            int persistentLocalId,
            IDictionary<Language, string> homonymAdditions,
            ProvenanceData provenance) :
            this(
                new MunicipalityId(municipalityId),
                new PersistentLocalId(persistentLocalId),
                new HomonymAdditions(homonymAdditions))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.AddRange(HomonymAdditions.Select(item => item.ToString()));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
