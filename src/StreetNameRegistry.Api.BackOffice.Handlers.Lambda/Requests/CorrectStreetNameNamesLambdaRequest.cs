namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record CorrectStreetNameNamesLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameNamesBackOfficeRequest>,
        IHasStreetNamePersistentLocalId
    {
        public CorrectStreetNameNamesLambdaRequest(string groupId, CorrectStreetNameNamesSqsRequest sqsRequest)
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

        public CorrectStreetNameNamesBackOfficeRequest Request { get; init; }

        /// <summary>
        /// Map to CorrectStreetNameNames command
        /// </summary>
        /// <returns>CorrectStreetNameNames.</returns>
        public CorrectStreetNameNames ToCommand()
        {
            var names = new Names(
                Request.Straatnamen.Select(x => new StreetNameName(x.Value, TaalMapper.ToLanguage(x.Key))));

            return new CorrectStreetNameNames(
                this.MunicipalityPersistentLocalId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                names,
                Provenance);
        }
    }
}
