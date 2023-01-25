namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectStreetNameRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameRetirementRequest>
    {
        public CorrectStreetNameRetirementRequest Request { get; init; }
    }
}
