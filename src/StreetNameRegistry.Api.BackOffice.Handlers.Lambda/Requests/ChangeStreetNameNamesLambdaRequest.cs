namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record ChangeStreetNameNamesLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<ChangeStreetNameNamesRequest>,
        IHasStreetNamePersistentLocalId
    {
        public ChangeStreetNameNamesLambdaRequest(string groupId, ChangeStreetNameNamesSqsRequest sqsRequest)
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

        public ChangeStreetNameNamesRequest Request { get; init; }

        /// <summary>
        /// Map to ChangeStreetNameNames command
        /// </summary>
        /// <returns>ChangeStreetNameNames.</returns>
        public ChangeStreetNameNames ToCommand()
        {
            var names = new Names(
                Request.Straatnamen.Select(x => new StreetNameName(x.Value, TaalMapper.ToLanguage(x.Key))));

            return new ChangeStreetNameNames(
                this.MunicipalityId(),
                new PersistentLocalId(StreetNamePersistentLocalId),
                names,
                Provenance);
        }
    }
}
