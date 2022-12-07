namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public sealed class OsloDetailHandlerV2 : OsloDetailHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public OsloDetailHandlerV2(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }
        public override async Task<StreetNameOsloResponse> Handle(OsloDetailRequest request, CancellationToken cancellationToken)
        {
            var streetNameV2 = await _legacyContext
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

            var gemeenteV2 = await GetStraatnaamDetailGemeente(_syndicationContext, streetNameV2.NisCode, _responseOptions.Value.GemeenteDetailUrl, cancellationToken);
            return new StreetNameOsloResponse(
                _responseOptions.Value.Naamruimte,
                _responseOptions.Value.ContextUrlDetail,
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
                streetNameV2.HomonymAdditionEnglish,
                streetNameV2.LastEventHash);
        }
    }
}
