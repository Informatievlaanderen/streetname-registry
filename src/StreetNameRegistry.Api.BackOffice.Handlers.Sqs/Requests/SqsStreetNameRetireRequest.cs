namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameRetireRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeRetireRequest>
    {
        public StreetNameBackOfficeRetireRequest Request { get; set; }
    }
}
