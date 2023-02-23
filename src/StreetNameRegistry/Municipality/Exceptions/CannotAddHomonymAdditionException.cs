namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class CannotAddHomonymAdditionException : StreetNameRegistryException
    {
        public Language Language { get; }

        public CannotAddHomonymAdditionException()
        {
        }

        public CannotAddHomonymAdditionException(Language language)
        {
            Language = language;
        }

        private CannotAddHomonymAdditionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
