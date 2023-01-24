namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameRejectionRequest>
    {
        public CorrectStreetNameRejectionRequest Request { get; init; }
    }
}
