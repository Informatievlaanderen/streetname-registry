namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SourceAndDestinationStreetNameAreTheSameException : StreetNameRegistryException
    {
        /// <summary>
        /// The streetname id which already exists
        /// </summary>
        public string StreetNameId { get; }

        public SourceAndDestinationStreetNameAreTheSameException()
        {
            StreetNameId = string.Empty;
        }

        public SourceAndDestinationStreetNameAreTheSameException(string streetNameId)
        {
            StreetNameId = streetNameId;
        }

        private SourceAndDestinationStreetNameAreTheSameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StreetNameId = string.Empty;
        }
    }
}
