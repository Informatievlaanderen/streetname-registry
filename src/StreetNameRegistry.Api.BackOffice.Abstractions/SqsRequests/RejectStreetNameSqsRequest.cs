namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RejectStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RejectStreetNameRequest>
    {
        public RejectStreetNameRequest Request { get; init; }
    }
}
