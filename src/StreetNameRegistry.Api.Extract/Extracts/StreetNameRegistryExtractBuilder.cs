namespace StreetNameRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.StreetNameExtract;
    using Projections.Syndication;
    using System.Linq;

    public class StreetNameRegistryExtractBuilder
    {
        public static ExtractFile CreateStreetNameFile(ExtractContext context, SyndicationContext syndicationContext)
        {
            var extractItems = context
                .StreetNameExtract
                .AsNoTracking()
                .Where(x => x.Complete)
                .OrderBy(x => x.StreetNamePersistentLocalId);

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

            return ExtractBuilder.CreateDbfFile<StreetNameExtractItem, StreetNameDbaseRecord>(
                ExtractController.ZipName,
                new StreetNameDbaseSchema(),
                extractItems,
                extractItems.Count,
                TransformRecord);
        }
    }
}
