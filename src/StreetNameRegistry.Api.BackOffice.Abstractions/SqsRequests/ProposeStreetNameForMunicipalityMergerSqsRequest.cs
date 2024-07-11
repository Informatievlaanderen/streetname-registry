namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeStreetNamesForMunicipalityMergerSqsRequest : SqsRequest
    {
        public string NisCode { get; set; }

        public List<ProposeStreetNamesForMunicipalityMergerSqsRequestItem> StreetNames { get; set; }

        public ProposeStreetNamesForMunicipalityMergerSqsRequest()
        { }

        public ProposeStreetNamesForMunicipalityMergerSqsRequest(
            string nisCode,
            List<ProposeStreetNamesForMunicipalityMergerSqsRequestItem> streetNames,
            ProvenanceData provenanceData)
        {
            NisCode = nisCode;
            StreetNames = streetNames;
            ProvenanceData = provenanceData;
        }
    }

    public sealed class ProposeStreetNamesForMunicipalityMergerSqsRequestItem
    {
        public int NewPersistentLocalId { get; set; }
        public string StreetName { get; set; }
        public string? HomonymAddition { get; set; }

        public List<int> MergedStreetNamePersistentLocalIds { get; set; }

        public ProposeStreetNamesForMunicipalityMergerSqsRequestItem()
        { }

        public ProposeStreetNamesForMunicipalityMergerSqsRequestItem(
            int newPersistentLocalId,
            string streetName,
            string? homonymAddition,
            List<int> mergedStreetNamePersistentLocalIds)
        {
            NewPersistentLocalId = newPersistentLocalId;
            StreetName = streetName;
            HomonymAddition = homonymAddition;
            MergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds;
        }
    }
}
