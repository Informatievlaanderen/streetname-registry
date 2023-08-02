namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameNameCorrectionExceededCharacterChangeLimitException : StreetNameRegistryException
    {
        public string StreetName { get; }

        public StreetNameNameCorrectionExceededCharacterChangeLimitException()
        {
            StreetName = string.Empty;
        }

        public StreetNameNameCorrectionExceededCharacterChangeLimitException(string name)
        {
            StreetName = name;
        }

        private StreetNameNameCorrectionExceededCharacterChangeLimitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StreetName = string.Empty;
        }
    }
}
