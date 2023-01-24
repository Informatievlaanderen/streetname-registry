namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RetireStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RetireStreetNameRequest>
    {
        public RetireStreetNameRequest Request { get; init; }
    }
}
