namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;
    using Municipality.Commands;

    public sealed class SqsLambdaStreetNameCorrectNamesRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<StreetNameBackOfficeCorrectNamesRequest>,
        IHasStreetNamePersistentLocalId
    {
        public int StreetNamePersistentLocalId { get; set; }

        public StreetNameBackOfficeCorrectNamesRequest Request { get; set; }

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
