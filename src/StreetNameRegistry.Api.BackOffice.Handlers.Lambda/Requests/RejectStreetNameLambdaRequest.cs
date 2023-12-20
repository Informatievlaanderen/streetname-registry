namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record RejectStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<RejectStreetNameRequest>,
        IHasStreetNamePersistentLocalId
    {
        public RejectStreetNameLambdaRequest(string groupId, RejectStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RejectStreetNameRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RejectStreetName command
        /// </summary>
        /// <returns>RejectStreetName.</returns>
        public RejectStreetName ToCommand(PersistentLocalId streetNamePersistentLocalId)
        {
            return new RejectStreetName(
                this.MunicipalityPersistentLocalId(),
                streetNamePersistentLocalId,
                CommandProvenance);
        }
    }
}
