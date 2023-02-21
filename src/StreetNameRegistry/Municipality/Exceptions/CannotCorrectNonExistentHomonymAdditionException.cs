namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class CannotCorrectNonExistentHomonymAdditionException : StreetNameRegistryException
    {
        public string LanguageCode { get; }

        public CannotCorrectNonExistentHomonymAdditionException()
        {
        }

        public CannotCorrectNonExistentHomonymAdditionException(string languageCode)
        {
            LanguageCode = languageCode;
        }

        private CannotCorrectNonExistentHomonymAdditionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
