namespace StreetNameRegistry.Api.Legacy.StreetName.Detail
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using global::Microsoft.EntityFrameworkCore;
    using MediatR;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;

    public sealed record DetailRequest(int PersistentLocalId) : IRequest<StreetNameResponse>;

    public abstract class DetailHandlerBase : IRequestHandler<DetailRequest, StreetNameResponse>
    {
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

        public abstract Task<StreetNameResponse> Handle(DetailRequest request, CancellationToken cancellationToken);
    }
}
