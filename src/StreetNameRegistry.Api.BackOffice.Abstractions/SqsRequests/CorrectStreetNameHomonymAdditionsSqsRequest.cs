namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectStreetNameHomonymAdditionsSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectStreetNameHomonymAdditionsRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectStreetNameHomonymAdditionsRequest Request { get; init; }
    }
}
