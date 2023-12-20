namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record ProposeStreetNameLambdaRequest :
        StreetNameLambdaRequest,
        IHasBackOfficeRequest<ProposeStreetNameRequest>
    {
        public ProposeStreetNameLambdaRequest(string groupId, ProposeStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            PersistentLocalId = new PersistentLocalId(sqsRequest.PersistentLocalId);
            Request = sqsRequest.Request;
        }

        public PersistentLocalId PersistentLocalId { get; }

        public ProposeStreetNameRequest Request { get; init; }

        /// <summary>
        /// Map to ProposeStreetName command
        /// </summary>
        /// <returns>ProposeStreetName.</returns>
        public ProposeStreetName ToCommand()
        {
            var names = new Names(
                Request.Straatnamen.Select(x => new StreetNameName(x.Value, TaalMapper.ToLanguage(x.Key))));

            return new ProposeStreetName(
                this.MunicipalityPersistentLocalId(),
                names,
                PersistentLocalId,
                CommandProvenance);
        }
    }
}
