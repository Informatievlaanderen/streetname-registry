namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsStreetNameCorrectApprovalRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeCorrectApprovalRequest>
    {
        public StreetNameBackOfficeCorrectApprovalRequest Request { get; set; }
    }
}
