namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;

    public sealed record CorrectStreetNameRetirementLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameRetirementRequest>,
        IHasStreetNamePersistentLocalId
    {
        public CorrectStreetNameRetirementLambdaRequest(string groupId, CorrectStreetNameRetirementSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectStreetNameRetirementRequest Request { get; init; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectStreetNameRetirement command
        /// </summary>
        /// <returns>CorrectStreetNameRetirement.</returns>
        public CorrectStreetNameRetirement ToCommand()
        {
            return new CorrectStreetNameRetirement(this.MunicipalityPersistentLocalId(), new PersistentLocalId(StreetNamePersistentLocalId), Provenance);
        }
    }
}
