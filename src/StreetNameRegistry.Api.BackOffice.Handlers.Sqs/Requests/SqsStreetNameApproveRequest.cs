namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameApproveRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeApproveRequest>
    {
        public StreetNameBackOfficeApproveRequest Request { get; set; }
    }
}
