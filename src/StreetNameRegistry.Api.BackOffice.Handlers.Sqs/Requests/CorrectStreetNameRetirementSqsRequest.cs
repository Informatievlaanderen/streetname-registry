namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectStreetNameRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameRetirementBackOfficeRequest>
    {
        public CorrectStreetNameRetirementBackOfficeRequest Request { get; init; }
    }
}
