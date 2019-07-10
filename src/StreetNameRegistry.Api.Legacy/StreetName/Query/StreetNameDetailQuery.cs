namespace StreetNameRegistry.Api.Legacy.StreetName.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Responses;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class StreetNameDetailQuery
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ResponseOptions _responseOptions;

        public StreetNameDetailQuery(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptionsProvider)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptionsProvider.Value;
        }

        public async Task<StreetNameResponse> FilterAsync(int persistentLocalId, CancellationToken ct = default)
        {
            var streetName = await _legacyContext
                .StreetNameDetail
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, ct);

            if (streetName == null)
                throw new ApiException("Onbestaande straatnaam.", StatusCodes.Status404NotFound);

            if (streetName.Removed)
                throw new ApiException("Straatnaam verwijderd.", StatusCodes.Status410Gone);

            var municipality = await _syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .OrderByDescending(m => m.Position)
                .FirstOrDefaultAsync(m => m.NisCode == streetName.NisCode, ct);

            var municipalityDefaultName = GetDefaultMunicipalityName(municipality);
            var gemeente = new StraatnaamDetailGemeente
            {
                ObjectId = streetName.NisCode,
                Detail = string.Format(_responseOptions.GemeenteDetailUrl, streetName.NisCode),
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam(municipalityDefaultName.Value, municipalityDefaultName.Key))
            };

            return new StreetNameResponse(
                _responseOptions.Naamruimte,
                streetName.PersistentLocalId.Value,
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

        private static KeyValuePair<Taal, string> GetDefaultMunicipalityName(MunicipalityLatestItem municipality)
        {
            switch (municipality.PrimaryLanguage)
            {
                default:
                case null:
                case Taal.NL:
                    return new KeyValuePair<Taal, string>(Taal.NL, municipality.NameDutch);
                case Taal.FR:
                    return new KeyValuePair<Taal, string>(Taal.FR, municipality.NameFrench);
                case Taal.DE:
                    return new KeyValuePair<Taal, string>(Taal.DE, municipality.NameGerman);
                case Taal.EN:
                    return new KeyValuePair<Taal, string>(Taal.EN, municipality.NameEnglish);
            }
        }
    }
}
