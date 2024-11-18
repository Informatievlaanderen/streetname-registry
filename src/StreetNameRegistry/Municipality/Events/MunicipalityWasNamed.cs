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
    [EventDescription("De gemeente werd benoemd.")]
    public sealed class MunicipalityWasNamed : IMunicipalityEvent
    {
        public const string EventName = "MunicipalityWasNamed"; // BE CAREFUL CHANGING THIS!!

        public Guid MunicipalityId { get; }
        public string Name { get; }
        public Language Language { get; set; }
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasNamed(MunicipalityId municipalityId, MunicipalityName name)
        {
            MunicipalityId = municipalityId;
            Name = name.Name;
            Language = name.Language;
        }

        [JsonConstructor]
        private MunicipalityWasNamed(
            Guid municipalityId,
            string name,
            Language language,
            ProvenanceData provenance)
            : this(new MunicipalityId(municipalityId), new MunicipalityName(name, language))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(Name);
            fields.Add(Language.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
