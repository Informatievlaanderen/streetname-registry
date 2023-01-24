namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameApprovalRequest>
    {
        public CorrectStreetNameApprovalRequest Request { get; init; }
    }
}
