namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record RetireStreetNameLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RetireStreetNameBackOfficeRequest>,
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

        public RetireStreetNameBackOfficeRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RetireStreetName command
        /// </summary>
        /// <returns>RetireStreetName.</returns>
        public RetireStreetName ToCommand(PersistentLocalId streetNamePersistentLocalId)
        {
            return new RetireStreetName(this.MunicipalityPersistentLocalId(), streetNamePersistentLocalId, Provenance);
        }
    }
}
