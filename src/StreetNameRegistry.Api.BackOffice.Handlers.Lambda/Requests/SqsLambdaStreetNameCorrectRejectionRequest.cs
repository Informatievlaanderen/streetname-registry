namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Requests;
    using Municipality;
    using Municipality.Commands;

    public sealed class SqsLambdaStreetNameCorrectRejectionRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<StreetNameBackOfficeCorrectRejectionRequest>,
        IHasStreetNamePersistentLocalId
    {
        public StreetNameBackOfficeCorrectRejectionRequest Request { get; set; }

        public int StreetNamePersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectStreetNameRejection command
        /// </summary>
        /// <returns>CorrectStreetNameRejection.</returns>
        public CorrectStreetNameRejection ToCommand()
        {
            return new CorrectStreetNameRejection(MunicipalityId, new PersistentLocalId(StreetNamePersistentLocalId), Provenance);
        }
    }
}
