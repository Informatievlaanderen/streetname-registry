namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record CorrectStreetNameHomonymAdditionsLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameHomonymAdditionsRequest>,
        IHasStreetNamePersistentLocalId
    {
        public CorrectStreetNameHomonymAdditionsLambdaRequest(string groupId, CorrectStreetNameHomonymAdditionsSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            StreetNamePersistentLocalId = sqsRequest.PersistentLocalId;
        }

        public int StreetNamePersistentLocalId { get; }

        public CorrectStreetNameHomonymAdditionsRequest Request { get; init; }

        /// <summary>
        /// Maps to command.
        /// </summary>
        /// <returns>CorrectStreetNameHomonymAdditions.</returns>
        public CorrectStreetNameHomonymAdditions ToCommand()
        {
            var homonymsToRemove = Request.HomoniemToevoegingen
                .Where(x => string.IsNullOrWhiteSpace(x.Value))
                .Select(x => x.Key.ToLanguage())
                .ToList();

            var homonymAdditionsToCorrect = new HomonymAdditions(
                Request.HomoniemToevoegingen
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .ToDictionary(x => TaalMapper.ToLanguage(x.Key), x => x.Value));

            return new CorrectStreetNameHomonymAdditions(
                this.MunicipalityPersistentLocalId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                homonymAdditionsToCorrect,
                homonymsToRemove,
                CommandProvenance);
        }
    }
}
