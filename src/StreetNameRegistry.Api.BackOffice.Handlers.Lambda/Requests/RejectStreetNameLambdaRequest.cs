namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record RejectStreetNameLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RejectStreetNameBackOfficeRequest>,
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

        public RejectStreetNameBackOfficeRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RejectStreetName command
        /// </summary>
        /// <returns>RejectStreetName.</returns>
        public RejectStreetName ToCommand(PersistentLocalId streetNamePersistentLocalId)
        {
            return new RejectStreetName(this.MunicipalityPersistentLocalId(), streetNamePersistentLocalId, Provenance);
        }
    }
}
