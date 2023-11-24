namespace StreetNameRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class StreetNameId : GuidValueObject<StreetNameId>
    {
        public static StreetNameId CreateFor(CrabStreetNameId crabStreetNameId)
            => new StreetNameId(crabStreetNameId.CreateDeterministicId());

        public StreetNameId([JsonProperty("value")] Guid id) : base(id) { }
    }
}
