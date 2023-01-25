namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectStreetNameApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameApprovalRequest>
    {
        public CorrectStreetNameApprovalRequest Request { get; init; }
    }
}
