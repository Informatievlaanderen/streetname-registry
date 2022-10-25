namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;
    using Sqs.Requests;

    public sealed record ProposeStreetNameLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<ProposeStreetNameBackOfficeRequest>
    {
        public ProposeStreetNameLambdaRequest(string groupId, ProposeStreetNameSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public ProposeStreetNameBackOfficeRequest Request { get; init; }

        /// <summary>
        /// Map to ProposeStreetName command
        /// </summary>
        /// <returns>ProposeStreetName.</returns>
        public ProposeStreetName ToCommand(
            PersistentLocalId persistentLocalId)
        {
            var names = new Names(
                Request.Straatnamen.Select(x => new StreetNameName(x.Value, TaalMapper.ToLanguage(x.Key))));

            return new ProposeStreetName(
                this.MunicipalityPersistentLocalId(),
                names,
                persistentLocalId,
                Provenance);
        }
    }
}
