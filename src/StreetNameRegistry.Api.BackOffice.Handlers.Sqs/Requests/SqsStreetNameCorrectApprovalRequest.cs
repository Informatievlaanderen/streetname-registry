namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameCorrectApprovalRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeCorrectApprovalRequest>
    {
        public StreetNameBackOfficeCorrectApprovalRequest Request { get; set; }
    }
}
