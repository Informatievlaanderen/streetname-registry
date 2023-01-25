namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ApproveStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ApproveStreetNameRequest>
    {
        public ApproveStreetNameRequest Request { get; init; }
    }
}
