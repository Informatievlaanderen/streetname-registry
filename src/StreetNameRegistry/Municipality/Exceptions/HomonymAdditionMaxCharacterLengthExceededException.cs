namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class HomonymAdditionMaxCharacterLengthExceededException : StreetNameRegistryException
    {
        public string LanguageCode { get; }

        public HomonymAdditionMaxCharacterLengthExceededException()
        {
        }

        public HomonymAdditionMaxCharacterLengthExceededException(string languageCode)
        {
            LanguageCode = languageCode;
        }

        private HomonymAdditionMaxCharacterLengthExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
