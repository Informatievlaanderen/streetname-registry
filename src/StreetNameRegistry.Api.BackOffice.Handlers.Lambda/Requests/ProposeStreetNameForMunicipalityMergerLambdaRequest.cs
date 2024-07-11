namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.SqsRequests;
    using Municipality;
    using Municipality.Commands;

    public sealed record ProposeStreetNameForMunicipalityMergerLambdaRequest : StreetNameLambdaRequest
    {
        private readonly IEnumerable<ProposeStreetNamesForMunicipalityMergerSqsRequestItem> _streetNames;
        public string NisCode { get; }

        public ProposeStreetNameForMunicipalityMergerLambdaRequest(
            string groupId,
            ProposeStreetNamesForMunicipalityMergerSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            _streetNames = sqsRequest.StreetNames;
            NisCode = sqsRequest.NisCode;
        }

        public IEnumerable<ProposeStreetNameForMunicipalityMerger> ToCommand()
        {
            return _streetNames.Select(x => new ProposeStreetNameForMunicipalityMerger(
                this.MunicipalityPersistentLocalId(),
                new Names(new[] {new StreetNameName(x.StreetName, Language.Dutch)}),
                x.HomonymAddition is not null
                    ? new HomonymAdditions(new[]{ new StreetNameHomonymAddition(x.HomonymAddition, Language.Dutch) })
                    : null,
                new PersistentLocalId(x.NewPersistentLocalId),
                x.MergedStreetNamePersistentLocalIds.Select(id => new PersistentLocalId(id)).ToList(),
                Provenance));
        }
    }
}
