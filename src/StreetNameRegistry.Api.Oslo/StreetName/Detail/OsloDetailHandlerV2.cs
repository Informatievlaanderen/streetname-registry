namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Converters;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;

    public sealed record OsloDetailRequest(int PersistentLocalId) : IRequest<StreetNameOsloResponse>;

    public sealed class OsloDetailHandlerV2 : IRequestHandler<OsloDetailRequest, StreetNameOsloResponse>
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
        public async Task<StreetNameOsloResponse> Handle(OsloDetailRequest request, CancellationToken cancellationToken)
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

        public async Task<StraatnaamDetailGemeente> GetStraatnaamDetailGemeente(SyndicationContext syndicationContext, string nisCode, string gemeenteDetailUrl, CancellationToken ct)
        {
            var municipality = await syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .OrderByDescending(m => m.Position)
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            var municipalityDefaultName = GetDefaultMunicipalityName(municipality);
            var gemeente = new StraatnaamDetailGemeente
            {
                ObjectId = nisCode,
                Detail = string.Format(gemeenteDetailUrl, nisCode),
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipalityDefaultName.Value, municipalityDefaultName.Key))
            };
            return gemeente;
        }

        private static KeyValuePair<Taal, string> GetDefaultMunicipalityName(MunicipalityLatestItem? municipality)
        {
            switch (municipality?.PrimaryLanguage)
            {
                default:
                case null:
                case Taal.NL:
                    return new KeyValuePair<Taal, string>(Taal.NL, municipality?.NameDutch ?? string.Empty);
                case Taal.FR:
                    return new KeyValuePair<Taal, string>(Taal.FR, municipality.NameFrench ?? string.Empty);
                case Taal.DE:
                    return new KeyValuePair<Taal, string>(Taal.DE, municipality.NameGerman ?? string.Empty);
                case Taal.EN:
                    return new KeyValuePair<Taal, string>(Taal.EN, municipality.NameEnglish ?? string.Empty);
            }
        }
    }
}
