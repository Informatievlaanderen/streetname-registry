namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ApproveStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ApproveStreetNameRequest>
    {
        public ApproveStreetNameRequest Request { get; init; }
    }
}
