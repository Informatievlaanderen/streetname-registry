namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameNamesSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameNamesRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectStreetNameNamesRequest Request { get; init; }
    }
}
