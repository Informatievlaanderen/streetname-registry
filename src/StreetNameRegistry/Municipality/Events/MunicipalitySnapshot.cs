namespace StreetNameRegistry.Municipality.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using DataStructures;
    using Newtonsoft.Json;

    [HideEvent]
    [EventName("MunicipalitySnapshot")]
    [EventSnapshot(nameof(SnapshotContainer) + "<MunicipalitySnapshot>", typeof(SnapshotContainer))]
    [EventDescription("Snapshot of Municipality with StreetNames")]
    public sealed class MunicipalitySnapshot
    {
        public Guid MunicipalityId { get; }
        public string NisCode { get; }
        public string MunicipalityStatus { get; }
        public IEnumerable<Language> OfficialLanguages { get; }
        public IEnumerable<Language> FacilityLanguages { get; }

        public IEnumerable<StreetNameData> StreetNames { get; }

        public MunicipalitySnapshot(
            MunicipalityId municipalityId,
            NisCode nisCode,
            MunicipalityStatus municipalityStatus,
            IEnumerable<Language> officialLanguages,
            IEnumerable<Language> facilityLanguages,
            MunicipalityStreetNames streetNames)
        {
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            MunicipalityStatus = municipalityStatus.Status;
            OfficialLanguages = officialLanguages;
            FacilityLanguages = facilityLanguages;
            StreetNames = streetNames.Select(x => new StreetNameData(x)).ToList();
        }

        [JsonConstructor]
        private MunicipalitySnapshot(
            Guid municipalityId,
            string nisCode,
            string municipalityStatus,
            IEnumerable<Language> officialLanguages,
            IEnumerable<Language> facilityLanguages,
            IEnumerable<StreetNameData> streetNames)
            : this(
                new MunicipalityId(municipalityId),
                new NisCode(nisCode),
                StreetNameRegistry.Municipality.MunicipalityStatus.Parse(municipalityStatus),
                officialLanguages,
                facilityLanguages,
                new MunicipalityStreetNames())
        {
            StreetNames = streetNames;
        }
    }
}
