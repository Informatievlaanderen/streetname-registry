namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class OsloDetailHandlerV2 : OsloDetailHandlerBase
    {
        public override async Task<IActionResult> Handle(OsloDetailRequest request, CancellationToken cancellationToken)
        {
            var streetNameV2 = await request.LegacyContext
                .StreetNameDetailV2
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (streetNameV2 == null)
            {
                throw new ApiException("Onbestaande straatnaam.", StatusCodes.Status404NotFound);
            }

            if (streetNameV2.Removed)
            {
                throw new ApiException("Straatnaam verwijderd.", StatusCodes.Status410Gone);
            }

            var gemeenteV2 = await GetStraatnaamDetailGemeente(request.SyndicationContext, streetNameV2.NisCode, request.ResponseOptions.Value.GemeenteDetailUrl, cancellationToken);
            var streetNameOsloResponse = new StreetNameOsloResponse(
                request.ResponseOptions.Value.Naamruimte,
                request.ResponseOptions.Value.ContextUrlDetail,
                request.PersistentLocalId,
                streetNameV2.Status.ConvertFromMunicipalityStreetNameStatus(),
                gemeenteV2,
                streetNameV2.VersionTimestamp.ToBelgianDateTimeOffset(),
                streetNameV2.NameDutch,
                streetNameV2.NameFrench,
                streetNameV2.NameGerman,
                streetNameV2.NameEnglish,
                streetNameV2.HomonymAdditionDutch,
                streetNameV2.HomonymAdditionFrench,
                streetNameV2.HomonymAdditionGerman,
                streetNameV2.HomonymAdditionEnglish);

            return string.IsNullOrWhiteSpace(streetNameV2.LastEventHash)
                ? new OkObjectResult(streetNameOsloResponse)
                : new OkWithLastObservedPositionAsETagResult(streetNameOsloResponse, streetNameV2.LastEventHash);
        }
    }
}
