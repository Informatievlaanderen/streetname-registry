namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RenameStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RenameStreetNameRequest>
    {
        public int PersistentLocalId { get; set; }

        public RenameStreetNameRequest Request { get; init; }
    }
}
