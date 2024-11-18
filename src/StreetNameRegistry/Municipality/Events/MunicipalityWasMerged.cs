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
    [EventDescription("De gemeente werd gefusioneerd.")]
    public sealed class MunicipalityWasMerged: IMunicipalityEvent
    {
        public const string EventName = "MunicipalityWasMerged"; // BE CAREFUL CHANGING THIS!!

        public Guid MunicipalityId { get; }
        public Guid NewMunicipalityId { get; }
        public ProvenanceData Provenance { get; private set; }

        public MunicipalityWasMerged(
            MunicipalityId municipalityId,
            MunicipalityId newMunicipalityId)
        {
            MunicipalityId = municipalityId;
            NewMunicipalityId = newMunicipalityId;
        }

        [JsonConstructor]
        private MunicipalityWasMerged(
            Guid municipalityId,
            Guid newMunicipalityId,
            ProvenanceData provenance)
            : this(
                new MunicipalityId(municipalityId),
                new MunicipalityId(newMunicipalityId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(NewMunicipalityId.ToString("D"));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
