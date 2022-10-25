namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameApprovalBackOfficeRequest>
    {
        public CorrectStreetNameApprovalBackOfficeRequest Request { get; init; }
    }
}
