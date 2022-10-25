namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RejectStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RejectStreetNameBackOfficeRequest>
    {
        public RejectStreetNameBackOfficeRequest Request { get; init; }
    }
}
