namespace StreetNameRegistry.Api.Oslo.StreetName.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam;
    using Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;

    public sealed record OsloDetailRequest(int PersistentLocalId) : IRequest<StreetNameOsloV3Response>;

    public sealed class OsloDetailHandler : IRequestHandler<OsloDetailRequest, StreetNameOsloV3Response>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public OsloDetailHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }
        public async Task<StreetNameOsloV3Response> Handle(OsloDetailRequest request, CancellationToken cancellationToken)
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
            return new StreetNameOsloV3Response(
                _responseOptions.Value.ContextUrlDetail,
                request.PersistentLocalId,
                streetNameV2.Status.ConvertOsloFromMunicipalityStreetNameStatus(),
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
                _responseOptions.Value.DetailUrl,
                _responseOptions.Value.StreetNameDetailAddressesLink,
                streetNameV2.LastEventHash);
        }

        private async Task<StraatnaamToegekendDoorGemeente> GetStraatnaamDetailGemeente(SyndicationContext syndicationContext, string nisCode, string gemeenteDetailUrl, CancellationToken ct)
        {
            var municipality = await syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .OrderByDescending(m => m.Position)
                .FirstAsync(m => m.NisCode == nisCode, ct);

            var municipalityNames = GetMunicipalityNames(municipality);
            var gemeente = new StraatnaamToegekendDoorGemeente
            {
                Id = OsloNamespaces.Gemeente.ToPuri(nisCode),
                Detail = new Uri(string.Format(gemeenteDetailUrl, nisCode)),
                Gemeentenaam = new Gemeentenaam
                {
                    Gemeentenamen = municipalityNames.ToList()
                }
            };
            return gemeente;
        }

        private static IEnumerable<GeografischeNaam> GetMunicipalityNames(MunicipalityLatestItem municipality)
        {
            var names = new List<GeografischeNaam>
            {
                new GeografischeNaam(municipality.NameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(municipality.NameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(municipality.NameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(municipality.NameEnglish ?? string.Empty, Taal.En)
            };

            return names.Where(name => !string.IsNullOrWhiteSpace(name.Spelling));
        }
    }
}
