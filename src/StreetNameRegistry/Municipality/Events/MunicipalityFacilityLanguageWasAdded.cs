namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [HideEvent]
    [EventTags(Tag.Municipality)]
    [EventName(EventName)]
    [EventDescription("Er werd een faciliteitentaal toegevoegd aan de gemeente.")]
    public sealed class MunicipalityFacilityLanguageWasAdded : IMunicipalityEvent
    {
        public const string EventName = "MunicipalityFacilityLanguageWasAdded"; // BE CAREFUL CHANGING THIS!!

        public Guid MunicipalityId { get; }
        public Language Language { get; }
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityFacilityLanguageWasAdded(MunicipalityId municipalityId, Language language)
        {
            MunicipalityId = municipalityId;
            Language = language;
        }

        [JsonConstructor]
        private MunicipalityFacilityLanguageWasAdded(
            Guid municipalityId,
            Language language,
            ProvenanceData provenance)
            : this(new MunicipalityId(municipalityId), language)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(Language.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
