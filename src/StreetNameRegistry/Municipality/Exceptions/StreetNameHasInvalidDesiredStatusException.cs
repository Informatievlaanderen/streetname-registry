namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StreetNameHasInvalidDesiredStatusException : StreetNameRegistryException
    {
        public PersistentLocalId PersistentLocalId { get; }

        public StreetNameHasInvalidDesiredStatusException()
        {
            PersistentLocalId = new PersistentLocalId(-1);
        }

        public StreetNameHasInvalidDesiredStatusException(PersistentLocalId persistentLocalId)
        {
            PersistentLocalId = persistentLocalId;
        }

        private StreetNameHasInvalidDesiredStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PersistentLocalId = new PersistentLocalId(-1);
        }
    }
}
