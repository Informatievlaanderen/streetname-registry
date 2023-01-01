namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using global::Microsoft.AspNetCore.Http;
    using global::Microsoft.EntityFrameworkCore;
    using global::Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public sealed class OsloDetailHandler : OsloDetailHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public OsloDetailHandler(
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

            return new StreetNameOsloResponse(
                _responseOptions.Value.Naamruimte,
                _responseOptions.Value.ContextUrlDetail,
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
