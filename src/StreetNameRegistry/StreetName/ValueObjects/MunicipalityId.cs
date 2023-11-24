namespace StreetNameRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class MunicipalityId : GuidValueObject<MunicipalityId>
    {
        public static MunicipalityId CreateFor(CrabMunicipalityId crabMunicipalityId)
            => new MunicipalityId(crabMunicipalityId.CreateDeterministicId());

        public static MunicipalityId CreateFor(string municipalityId)
            => new MunicipalityId(Guid.Parse(municipalityId));

        public MunicipalityId([JsonProperty("value")] Guid municipalityId) : base(municipalityId) { }
    }
}
