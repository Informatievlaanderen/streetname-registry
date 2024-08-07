namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record RemoveStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<RemoveStreetNameRequest>,
        IHasStreetNamePersistentLocalId
    {
        public RemoveStreetNameLambdaRequest(string groupId, RemoveStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RemoveStreetNameRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>RemoveStreetName.</returns>
        public RemoveStreetName ToCommand()
        {
            return new RemoveStreetName(
                this.MunicipalityId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                Provenance);
        }
    }
}
