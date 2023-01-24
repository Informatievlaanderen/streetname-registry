namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeStreetNameRequest>
    {
        public ProposeStreetNameRequest Request { get; init; }
    }
}
