namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;

    public sealed class SqsLambdaStreetNameRetireRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<StreetNameBackOfficeRetireRequest>,
        IHasStreetNamePersistentLocalId
    {
        public StreetNameBackOfficeRetireRequest Request { get; set; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RetireStreetName command
        /// </summary>
        /// <returns>RetireStreetName.</returns>
        public RetireStreetName ToCommand(PersistentLocalId streetNamePersistentLocalId)
        {
            return new RetireStreetName(this.MunicipalityPersistentLocalId(), streetNamePersistentLocalId, Provenance);
        }
    }
}
