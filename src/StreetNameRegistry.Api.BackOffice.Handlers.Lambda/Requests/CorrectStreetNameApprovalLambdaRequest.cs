namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record CorrectStreetNameApprovalLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameApprovalBackOfficeRequest>,
        IHasStreetNamePersistentLocalId
    {
        public CorrectStreetNameApprovalLambdaRequest(string groupId, CorrectStreetNameApprovalSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectStreetNameApprovalBackOfficeRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectStreetNameApproval command
        /// </summary>
        /// <returns>CorrectStreetNameApproval.</returns>
        public CorrectStreetNameApproval ToCommand()
        {
            return new CorrectStreetNameApproval(this.MunicipalityPersistentLocalId(), new PersistentLocalId(StreetNamePersistentLocalId), Provenance);
        }
    }
}
