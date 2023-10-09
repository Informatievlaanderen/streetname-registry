namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Syndication;
    using Query;
    using StreetNameRegistry.StreetName;

    public sealed class OsloListHandler : OsloListHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public OsloListHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            ConsumerPostalContext postalContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _postalContext = postalContext;
            _responseOptions = responseOptions;
        }

        public override async Task<StreetNameListOsloResponse> Handle(OsloListRequest request, CancellationToken cancellationToken)
        {
            var streetNameQuery = new StreetNameListOsloQuery(_legacyContext, _syndicationContext, _postalContext)
                .Fetch<StreetNameListItem, StreetNameListItem>(request.Filtering, request.Sorting, request.PaginationRequest);

            var pagedStreetNames = await streetNameQuery
                .Items
                .Select(m => new StreetNameListOsloItemResponse(
                    m.PersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    GetGeografischeNaamByTaal(m, m.PrimaryLanguage),
                    GetHomoniemToevoegingByTaal(m, m.PrimaryLanguage),
                    m.Status.ConvertFromStreetNameStatus(),
                    m.VersionTimestamp.ToBelgianDateTimeOffset()))
                .ToListAsync(cancellationToken);

            return
                new StreetNameListOsloResponse
                {
                    Straatnamen = pagedStreetNames,
                    Volgende = BuildNextUri(streetNameQuery.PaginationInfo, pagedStreetNames.Count,  _responseOptions.Value.VolgendeUrl),
                    Context = _responseOptions.Value.ContextUrlList,
                    Sorting = streetNameQuery.Sorting,
                    Pagination = streetNameQuery.PaginationInfo
                };
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListItem item, Language? taal)
        {
            switch (taal)
            {
                case null when !string.IsNullOrEmpty(item.NameDutch):
                case Language.Dutch when !string.IsNullOrEmpty(item.NameDutch):
                    return new GeografischeNaam(
                        item.NameDutch,
                        Taal.NL);

                case Language.French when !string.IsNullOrEmpty(item.NameFrench):
                    return new GeografischeNaam(
                        item.NameFrench,
                        Taal.FR);

                case Language.German when !string.IsNullOrEmpty(item.NameGerman):
                    return new GeografischeNaam(
                        item.NameGerman,
                        Taal.DE);

                case Language.English when !string.IsNullOrEmpty(item.NameEnglish):
                    return new GeografischeNaam(
                        item.NameEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }

        private static GeografischeNaam? GetHomoniemToevoegingByTaal(StreetNameListItem item, Language? taal)
        {
            switch (taal)
            {
                case null when !string.IsNullOrEmpty(item.HomonymAdditionDutch):
                case Language.Dutch when !string.IsNullOrEmpty(item.HomonymAdditionDutch):
                    return new GeografischeNaam(
                        item.HomonymAdditionDutch,
                        Taal.NL);

                case Language.French when !string.IsNullOrEmpty(item.HomonymAdditionFrench):
                    return new GeografischeNaam(
                        item.HomonymAdditionFrench,
                        Taal.FR);

                case Language.German when !string.IsNullOrEmpty(item.HomonymAdditionGerman):
                    return new GeografischeNaam(
                        item.HomonymAdditionGerman,
                        Taal.DE);

                case Language.English when !string.IsNullOrEmpty(item.HomonymAdditionEnglish):
                    return new GeografischeNaam(
                        item.HomonymAdditionEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }
    }
}
