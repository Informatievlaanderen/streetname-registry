namespace StreetNameRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Municipality.Exceptions;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class NisCode : StringValueObject<NisCode>
    {
        public NisCode([JsonProperty("value")] string nisCode) : base(nisCode)
        {
            if (string.IsNullOrWhiteSpace(nisCode))
                throw new NoNisCodeHasNoValueException("NisCode of a municipality cannot be empty.");
        }
    }
}
