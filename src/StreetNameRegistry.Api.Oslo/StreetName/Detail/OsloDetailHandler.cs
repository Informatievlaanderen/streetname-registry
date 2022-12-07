namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class OsloDetailHandler : OsloDetailHandlerBase
    {
        public override async Task<IActionResult> Handle(OsloDetailRequest request, CancellationToken cancellationToken)
        {
            var streetName = await request.LegacyContext
                .StreetNameDetail
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (streetName == null)
            {
                throw new ApiException("Onbestaande straatnaam.", StatusCodes.Status404NotFound);
            }

            if (streetName.Removed)
            {
                throw new ApiException("Straatnaam verwijderd.", StatusCodes.Status410Gone);
            }

            var gemeente = await GetStraatnaamDetailGemeente(request.SyndicationContext, streetName.NisCode, request.ResponseOptions.Value.GemeenteDetailUrl, cancellationToken);

            return new OkObjectResult(new StreetNameOsloResponse(
                request.ResponseOptions.Value.Naamruimte,
                request.ResponseOptions.Value.ContextUrlDetail,
                request.PersistentLocalId,
                streetName.Status.ConvertFromStreetNameStatus(),
                gemeente,
                streetName.VersionTimestamp.ToBelgianDateTimeOffset(),
                streetName.NameDutch,
                streetName.NameFrench,
                streetName.NameGerman,
                streetName.NameEnglish,
                streetName.HomonymAdditionDutch,
                streetName.HomonymAdditionFrench,
                streetName.HomonymAdditionGerman,
                streetName.HomonymAdditionEnglish));
        }
    }
}
