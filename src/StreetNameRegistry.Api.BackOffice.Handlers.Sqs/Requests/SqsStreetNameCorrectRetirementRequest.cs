namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsStreetNameCorrectRetirementRequest : SqsRequest, IHasBackOfficeRequest<StreetNameBackOfficeCorrectRetirementRequest>
    {
        public StreetNameBackOfficeCorrectRetirementRequest Request { get; set; }
    }
}
