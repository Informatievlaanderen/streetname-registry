namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RetireStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RetireStreetNameRequest>
    {
        public RetireStreetNameRequest Request { get; init; }
    }
}
