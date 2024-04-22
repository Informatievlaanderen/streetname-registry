namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameIsRenamedException : StreetNameRegistryException
    {
        public PersistentLocalId PersistentLocalId { get; }

        public StreetNameIsRenamedException()
        {
            PersistentLocalId = new PersistentLocalId(-1);
        }

        public StreetNameIsRenamedException(PersistentLocalId persistentLocalId)
        {
            PersistentLocalId = persistentLocalId;
        }

        private StreetNameIsRenamedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PersistentLocalId = new PersistentLocalId(-1);
        }
    }
}
