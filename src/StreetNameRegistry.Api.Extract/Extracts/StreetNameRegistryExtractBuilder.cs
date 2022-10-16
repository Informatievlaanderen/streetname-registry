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
        public static IEnumerable<ExtractFile> CreateStreetNameFiles(ExtractContext context,
            SyndicationContext syndicationContext)
        {
            var extractItems = context
                .StreetNameExtract
                .AsNoTracking()
                .Where(x => x.Complete)
                .OrderBy(x => x.StreetNamePersistentLocalId);

            var streetNameProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(StreetNameExtractProjections).FullName);
            var extractMetadata = new Dictionary<string, string>
            {
                {ExtractMetadataKeys.LatestEventId, streetNameProjectionState.Position.ToString()}
            };

            var cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();

            byte[] TransformRecord(StreetNameExtractItem r)
            {
                var item = new StreetNameDbaseRecord();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<StreetNameDbaseRecord>.Encoding);

                var municipality = cachedMunicipalities.First(x => x.NisCode == item.gemeenteid.Value);

                switch (municipality.PrimaryLanguage)
                {
                    case null:
                    default:
                        item.straatnm.Value = r.NameUnknown;
                        item.homoniemtv.Value = r.HomonymUnknown ?? string.Empty;
                        break;

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

                return item.ToBytes(DbfFileWriter<StreetNameDbaseRecord>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<StreetNameExtractItem, StreetNameDbaseRecord>(
                ExtractController.ZipName,
                new StreetNameDbaseSchema(),
                extractItems,
                extractItems.Count,
                TransformRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractController.ZipName,
                extractMetadata);
        }

        // Only use in staging
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
                var item = new StreetNameDbaseRecord();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<StreetNameDbaseRecord>.Encoding);

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

                return item.ToBytes(DbfFileWriter<StreetNameDbaseRecord>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<StreetNameExtractItemV2, StreetNameDbaseRecord>(
                ExtractController.ZipName,
                new StreetNameDbaseSchema(),
                extractItems,
                extractItems.Count,
                TransformRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractController.ZipName,
                extractMetadata);
        }
    }
}
