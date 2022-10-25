namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeStreetNameBackOfficeRequest>
    {
        public ProposeStreetNameBackOfficeRequest Request { get; init; }
    }
}
