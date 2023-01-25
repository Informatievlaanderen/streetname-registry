namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectStreetNameNamesSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameNamesRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectStreetNameNamesRequest Request { get; init; }
    }
}
