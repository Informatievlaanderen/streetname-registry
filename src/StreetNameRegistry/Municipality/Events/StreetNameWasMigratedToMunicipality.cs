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

    [EventTags(EventTag.For.Sync, Tag.Migration)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd gemigreerd naar gemeente.")]
    public sealed class StreetNameWasMigratedToMunicipality : IMunicipalityEvent, IHasPersistentLocalId
    {
        public const string EventName = "StreetNameWasMigratedToMunicipality"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public Guid MunicipalityId { get; }
        [EventPropertyDescription("NIS-code (= objectidentificator) van de gemeente aan dewelke de straatnaam is toegewezen.")]
        public string NisCode { get; }
        [EventPropertyDescription("Interne GUID van de gemigreerde straatnaam.")]
        public Guid StreetNameId { get; }
        [EventPropertyDescription("Objectidentificator van de straatnaam.")]
        public int PersistentLocalId { get; }
        [EventPropertyDescription("De status van de straatnaam. Mogelijkheden: Current, Proposed en Retired.")]
        public StreetNameStatus Status { get; }
        [EventPropertyDescription("Taal waarin de officiële naam staat. Mogelijkheden: Dutch, French, German of English.")]
        public Language? PrimaryLanguage { get; }
        [EventPropertyDescription("Taal waarin de officiële naam staat. Mogelijkheden: Dutch, French, German of English.")]
        public Language? SecondaryLanguage { get; }
        [EventPropertyDescription("De straatnamen in de officiële en (eventuele) faciliteitentaal van de gemeente. Mogelijkheden: Dutch, French, German of English.")]
        public IDictionary<Language, string> Names { get; }
        [EventPropertyDescription("Homoniemtoevoeging aan de straatnaam.")]
        public IDictionary<Language, string> HomonymAdditions { get; }
        [EventPropertyDescription("De inhoud is altijd true en is wanneer de straatnaam voldoet aan het informatiemodel. ")]
        public bool IsCompleted { get; }
        [EventPropertyDescription("False wanneer de straatnaam niet werd verwijderd. True wanneer de straatnaam werd verwijderd.")]
        public bool IsRemoved { get; }
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasMigratedToMunicipality(
            MunicipalityId municipalityId,
            NisCode nisCode,
            StreetNameId streetNameId,
            PersistentLocalId persistentLocalId,
            StreetNameStatus status,
            Language? primaryLanguage,
            Language? secondaryLanguage,
            Names names,
            HomonymAdditions homonymAdditions,
            bool isCompleted,
            bool isRemoved)
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            StreetNameId = streetNameId;
            PersistentLocalId = persistentLocalId;
            Status = status;
            PrimaryLanguage = primaryLanguage;
            SecondaryLanguage = secondaryLanguage;
            Names = names.ToDictionary();
            HomonymAdditions = homonymAdditions.ToDictionary();
            IsCompleted = isCompleted;
            IsRemoved = isRemoved;
        }

        [JsonConstructor]
        private StreetNameWasMigratedToMunicipality(
            Guid municipalityId,
            string nisCode,
            Guid streetNameId,
            int persistentLocalId,
            StreetNameStatus status,
            Language? primaryLanguage,
            Language? secondaryLanguage,
            IDictionary<Language, string> names,
            IDictionary<Language, string> homonymAdditions,
            bool isCompleted,
            bool isRemoved,
            ProvenanceData provenance)
            : this(
                new MunicipalityId(municipalityId),
                new NisCode(nisCode),
                new StreetNameId(streetNameId),
                new PersistentLocalId(persistentLocalId),
                status,
                primaryLanguage,
                secondaryLanguage,
                new Names(names),
                new HomonymAdditions(homonymAdditions),
                isCompleted,
                isRemoved)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(NisCode);
            fields.Add(StreetNameId.ToString("D"));
            fields.Add(PersistentLocalId.ToString());
            fields.Add(Status.ToString());
            fields.Add(PrimaryLanguage?.ToString() ?? string.Empty);
            fields.Add(SecondaryLanguage?.ToString() ?? string.Empty);
            fields.AddRange(Names.Select(x => x.Key + x.Value));
            fields.AddRange(HomonymAdditions.Select(x => x.Key + x.Value));
            fields.Add(IsCompleted.ToString());
            fields.Add(IsRemoved.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
