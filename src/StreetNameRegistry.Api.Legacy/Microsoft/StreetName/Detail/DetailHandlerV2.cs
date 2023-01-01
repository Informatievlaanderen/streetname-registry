namespace StreetNameRegistry.Api.Legacy.Microsoft.StreetName.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using global::Microsoft.AspNetCore.Http;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.Extensions.Options;
    using Convertors;
    using Infrastructure.Options;
    using StreetNameRegistry.Projections.Legacy.Microsoft;
    using StreetNameRegistry.Projections.Syndication.Microsoft;

    public sealed class DetailHandlerV2 : DetailHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public DetailHandlerV2(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public override async Task<StreetNameResponse> Handle(DetailRequest request, CancellationToken cancellationToken)
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
            return new StreetNameResponse(
                _responseOptions.Value.Naamruimte,
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
