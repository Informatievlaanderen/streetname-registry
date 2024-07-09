namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class MergedStreetNamePersistentLocalIdsAreNotUniqueException : StreetNameRegistryException
    {
        public MergedStreetNamePersistentLocalIdsAreNotUniqueException()
        { }

        private MergedStreetNamePersistentLocalIdsAreNotUniqueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
