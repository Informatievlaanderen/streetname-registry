namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameRejectRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeRejectRequest>
    {
        public StreetNameBackOfficeRejectRequest Request { get; set; }
    }
}
