namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNamesAreActiveException : StreetNameRegistryException
    {
        public StreetNamesAreActiveException()
        { }

        public StreetNamesAreActiveException(PersistentLocalId persistentLocalId)
        { }

        private StreetNamesAreActiveException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
