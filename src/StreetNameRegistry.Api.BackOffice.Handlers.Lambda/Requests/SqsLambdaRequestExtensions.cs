namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Municipality;

    public static class SqsLambdaRequestExtensions
    {
        public static MunicipalityId MunicipalityPersistentLocalId(this SqsLambdaRequest request) =>
            StreetNameRegistry.Municipality.MunicipalityId.CreateFor(request.MessageGroupId);
    }
}
