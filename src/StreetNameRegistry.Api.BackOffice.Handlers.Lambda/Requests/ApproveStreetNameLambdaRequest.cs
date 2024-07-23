namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record ApproveStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<ApproveStreetNameRequest>,
        IHasStreetNamePersistentLocalId
    {
        public ApproveStreetNameLambdaRequest(string groupId, ApproveStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public ApproveStreetNameRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to ApproveStreetName command
        /// </summary>
        /// <returns>ApproveStreetName.</returns>
        public ApproveStreetName ToCommand()
        {
            return new ApproveStreetName(
                this.MunicipalityId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                Provenance);
        }
    }
}
