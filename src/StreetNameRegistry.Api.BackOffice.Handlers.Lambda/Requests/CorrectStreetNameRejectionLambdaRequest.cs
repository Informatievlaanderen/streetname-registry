namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record CorrectStreetNameRejectionLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameRejectionRequest>,
        IHasStreetNamePersistentLocalId
    {
        public CorrectStreetNameRejectionLambdaRequest(string groupId, CorrectStreetNameRejectionSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectStreetNameRejectionRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectStreetNameRejection command
        /// </summary>
        /// <returns>CorrectStreetNameRejection.</returns>
        public CorrectStreetNameRejection ToCommand()
        {
            return new CorrectStreetNameRejection(
                this.MunicipalityPersistentLocalId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                Provenance);
        }
    }
}
