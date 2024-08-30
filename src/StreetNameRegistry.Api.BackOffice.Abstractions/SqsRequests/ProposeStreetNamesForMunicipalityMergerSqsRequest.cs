namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using System;
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
        public List<MergedStreetName> MergedStreetNames { get; set; }

        public ProposeStreetNamesForMunicipalityMergerSqsRequestItem()
        { }

        public ProposeStreetNamesForMunicipalityMergerSqsRequestItem(
            int newPersistentLocalId,
            string streetName,
            string? homonymAddition,
            List<MergedStreetName> mergedStreetNames)
        {
            NewPersistentLocalId = newPersistentLocalId;
            StreetName = streetName;
            HomonymAddition = homonymAddition;
            MergedStreetNames = mergedStreetNames;
        }
    }

    public sealed class MergedStreetName
    {
        public int StreetNamePersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }

        public MergedStreetName()
        { }

        public MergedStreetName(int streetNamePersistentLocalId, Guid municipalityId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            MunicipalityId = municipalityId;
        }
    }
}
