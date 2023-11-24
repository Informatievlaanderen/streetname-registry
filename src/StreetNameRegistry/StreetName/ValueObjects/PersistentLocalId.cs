namespace StreetNameRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class PersistentLocalId : IntegerValueObject<PersistentLocalId>
    {
        public PersistentLocalId([JsonProperty("value")] int persistentLocalId) : base(persistentLocalId) {}
    }
}
