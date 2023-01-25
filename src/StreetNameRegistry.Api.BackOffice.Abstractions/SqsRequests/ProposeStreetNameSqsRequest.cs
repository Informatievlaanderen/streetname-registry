namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ProposeStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeStreetNameRequest>
    {
        public ProposeStreetNameRequest Request { get; init; }
    }
}
