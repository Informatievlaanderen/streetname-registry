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
    [EventDescription("De gemeente werd gecorrigeerd tot in gebruik.")]
    public sealed class MunicipalityWasCorrectedToCurrent : IMunicipalityEvent
    {
        public const string EventName = "MunicipalityWasCorrectedToCurrent"; // BE CAREFUL CHANGING THIS!!

        public Guid MunicipalityId { get; }
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasCorrectedToCurrent(MunicipalityId municipalityId)
        {
            MunicipalityId = municipalityId;
        }

        [JsonConstructor]
        private MunicipalityWasCorrectedToCurrent(
            Guid municipalityId,
            ProvenanceData provenance)
            : this(new MunicipalityId(municipalityId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
