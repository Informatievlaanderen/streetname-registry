namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;
    using StreetNameRegistry.Municipality;
    using System.Collections.Generic;

    public sealed class ProposeStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeStreetNameRequest>
    {
        public int PersistentLocalId { get; set; }

        public ProposeStreetNameRequest Request { get; init; }
    }

    public sealed class ProposeStreetNameRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _idGenerator;

        public ProposeStreetNameRequestFactory(IPersistentLocalIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public ProposeStreetNameSqsRequest Create(ProposeStreetNameRequest request, IDictionary<string, object?> metaData, ProvenanceData provenanceData)
        {
            return new ProposeStreetNameSqsRequest
            {
                PersistentLocalId = _idGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metaData,
                ProvenanceData = provenanceData,
            };
        }
    }
}
