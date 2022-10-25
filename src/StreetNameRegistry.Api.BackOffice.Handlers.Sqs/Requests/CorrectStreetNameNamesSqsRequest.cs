namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameNamesSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameNamesBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectStreetNameNamesBackOfficeRequest Request { get; init; }
    }
}
