namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record RetireStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<RetireStreetNameRequest>,
        IHasStreetNamePersistentLocalId
    {
        public RetireStreetNameLambdaRequest(string groupId, RetireStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RetireStreetNameRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RetireStreetName command
        /// </summary>
        /// <returns>RetireStreetName.</returns>
        public RetireStreetName ToCommand(PersistentLocalId streetNamePersistentLocalId)
        {
            return new RetireStreetName(
                this.MunicipalityId(),
                streetNamePersistentLocalId,
                Provenance);
        }
    }
}
