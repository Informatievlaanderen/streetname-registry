namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameCorrectNamesRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeCorrectNamesRequest>
    {
        public int PersistentLocalId { get; set; }

        public StreetNameBackOfficeCorrectNamesRequest Request { get; set; }
    }
}
