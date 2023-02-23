namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class HomonymAdditionMaxCharacterLengthExceededException : StreetNameRegistryException
    {
        public Language Language { get; }

        public HomonymAdditionMaxCharacterLengthExceededException()
        {
        }

        public HomonymAdditionMaxCharacterLengthExceededException(Language language)
        {
            Language = language;
        }

        private HomonymAdditionMaxCharacterLengthExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
