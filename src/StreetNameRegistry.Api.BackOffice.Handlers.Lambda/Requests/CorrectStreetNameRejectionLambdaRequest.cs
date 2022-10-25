namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record CorrectStreetNameRejectionLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameRejectionBackOfficeRequest>,
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

        public CorrectStreetNameRejectionBackOfficeRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectStreetNameRejection command
        /// </summary>
        /// <returns>CorrectStreetNameRejection.</returns>
        public CorrectStreetNameRejection ToCommand()
        {
            return new CorrectStreetNameRejection(this.MunicipalityPersistentLocalId(), new PersistentLocalId(StreetNamePersistentLocalId), Provenance);
        }
    }
}
