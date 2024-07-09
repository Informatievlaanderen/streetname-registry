namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class MergedStreetNamePersistentLocalIdsAreMissingException : StreetNameRegistryException
    {
        public MergedStreetNamePersistentLocalIdsAreMissingException()
        { }

        private MergedStreetNamePersistentLocalIdsAreMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
