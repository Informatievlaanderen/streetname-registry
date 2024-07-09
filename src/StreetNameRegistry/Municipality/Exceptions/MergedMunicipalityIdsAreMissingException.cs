namespace StreetNameRegistry.Municipality.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class MergedMunicipalityIdsAreMissingException : StreetNameRegistryException
    {
        public MergedMunicipalityIdsAreMissingException()
        { }

        private MergedMunicipalityIdsAreMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
