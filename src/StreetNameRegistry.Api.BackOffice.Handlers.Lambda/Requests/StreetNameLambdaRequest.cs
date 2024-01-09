namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;

    public abstract record StreetNameLambdaRequest : SqsLambdaRequest
    {
        protected StreetNameLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        { }
    }
}
