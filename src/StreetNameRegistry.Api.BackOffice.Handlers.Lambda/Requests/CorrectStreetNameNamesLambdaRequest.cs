namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record CorrectStreetNameNamesLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<CorrectStreetNameNamesRequest>,
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

        public CorrectStreetNameNamesRequest Request { get; init; }

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
