namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectStreetNameRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameRejectionRequest>
    {
        public CorrectStreetNameRejectionRequest Request { get; init; }
    }
}
