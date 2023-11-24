namespace StreetNameRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.StreetNameExtract;
    using Projections.Syndication;
    using System.Linq;

    public static class StreetNameRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateStreetNameFilesV2(ExtractContext context,
            SyndicationContext syndicationContext)
        {
            var extractItems = context
                .StreetNameExtractV2
                .AsNoTracking()
                .OrderBy(x => x.StreetNamePersistentLocalId);

            var streetNameProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(StreetNameExtractProjectionsV2).FullName);
            var extractMetadata = new Dictionary<string, string>
            {
                {ExtractMetadataKeys.LatestEventId, streetNameProjectionState.Position.ToString()}
            };

            var cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();

            byte[] TransformRecord(StreetNameExtractItemV2 r)
            {
                var item = new StreetNameDbaseRecordV2();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<StreetNameDbaseRecordV2>.Encoding);

                var municipality = cachedMunicipalities.First(x => x.NisCode == item.gemeenteid.Value);

                switch (municipality.PrimaryLanguage)
                {
                    case Taal.NL:
                        item.straatnm.Value = r.NameDutch;
                        item.homoniemtv.Value = r.HomonymDutch ?? string.Empty;
                        break;

                    case Taal.FR:
                        item.straatnm.Value = r.NameFrench;
                        item.homoniemtv.Value = r.HomonymFrench ?? string.Empty;
                        break;

                    case Taal.DE:
                        item.straatnm.Value = r.NameGerman;
                        item.homoniemtv.Value = r.HomonymGerman ?? string.Empty;
                        break;

                    case Taal.EN:
                        item.straatnm.Value = r.NameEnglish;
                        item.homoniemtv.Value = r.HomonymEnglish ?? string.Empty;
                        break;
                }

                return item.ToBytes(DbfFileWriter<StreetNameDbaseRecordV2>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<StreetNameExtractItemV2, StreetNameDbaseRecordV2>(
                ExtractController.ZipName,
                new StreetNameDbaseSchemaV2(),
                extractItems,
                extractItems.Count,
                TransformRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractController.ZipName,
                extractMetadata);
        }
    }
}
