namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameCorrectRejectionRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeCorrectRejectionRequest>
    {
        public StreetNameBackOfficeCorrectRejectionRequest Request { get; set; }
    }
}
