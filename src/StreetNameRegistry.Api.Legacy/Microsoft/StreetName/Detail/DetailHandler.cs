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

    public sealed class DetailHandler : DetailHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public DetailHandler(
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
            var streetName = await _legacyContext
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

            var gemeente = await GetStraatnaamDetailGemeente(_syndicationContext, streetName.NisCode, _responseOptions.Value.GemeenteDetailUrl, cancellationToken);

            return new StreetNameResponse(
                _responseOptions.Value.Naamruimte,
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
                streetName.HomonymAdditionEnglish);
        }
    }
}
