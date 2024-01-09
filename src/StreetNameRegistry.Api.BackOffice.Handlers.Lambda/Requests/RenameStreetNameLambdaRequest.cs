namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Municipality;
    using Municipality.Commands;

    public sealed record RenameStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<RenameStreetNameRequest>,
        IHasStreetNamePersistentLocalId
    {
        public RenameStreetNameLambdaRequest(string groupId, RenameStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            StreetNamePersistentLocalId = sqsRequest.PersistentLocalId;
        }

        public RenameStreetNameRequest Request { get; init; }

        public int StreetNamePersistentLocalId { get; }

        /// <summary>
        /// Map to RenameStreetName command
        /// </summary>
        /// <returns>RenameStreetName.</returns>
        public RenameStreetName ToCommand()
        {
            var identifier = Request.DoelStraatnaamId
                .AsIdentifier()
                .Map(int.Parse);

            return new RenameStreetName(
                this.MunicipalityPersistentLocalId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                new PersistentLocalId(identifier.Value),
                Provenance);
        }
    }
}
