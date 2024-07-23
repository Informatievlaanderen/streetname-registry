namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.SqsRequests;

    public sealed record ProposeStreetNameForMunicipalityMergerLambdaRequest : StreetNameLambdaRequest
    {
        public IEnumerable<ProposeStreetNamesForMunicipalityMergerSqsRequestItem> StreetNames { get; }
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
            StreetNames = sqsRequest.StreetNames;
            NisCode = sqsRequest.NisCode;
        }
    }
}
