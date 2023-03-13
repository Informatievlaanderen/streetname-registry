namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ChangeStreetNameNamesSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeStreetNameNamesRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangeStreetNameNamesRequest Request { get; init; }
    }
}
