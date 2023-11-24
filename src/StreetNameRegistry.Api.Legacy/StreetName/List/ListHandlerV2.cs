namespace StreetNameRegistry.Api.Legacy.StreetName.List
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using Convertors;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Query;

    public sealed record ListRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting, IPaginationRequest Pagination) : IRequest<StreetNameListResponse>;

    public sealed class ListHandlerV2 : IRequestHandler<ListRequest, StreetNameListResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        private readonly IOptions<ResponseOptions> _responseOptions;

        public ListHandlerV2(
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

        public async Task<StreetNameListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var streetNameQuery = new StreetNameListQueryV2(_legacyContext, _syndicationContext, _postalContext)
                    .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, request.Pagination);

            var pagedStreetNames = await streetNameQuery
                .Items
                .Select(m => new StreetNameListItemResponse(
                    m.PersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    GetGeografischeNaamByTaal(m, m.PrimaryLanguage),
                    GetHomoniemToevoegingByTaal(m, m.PrimaryLanguage),
                    m.Status.ConvertFromMunicipalityStreetNameStatus(),
                    m.VersionTimestamp.ToBelgianDateTimeOffset()))
                .ToListAsync(cancellationToken);

            return
                new StreetNameListResponse
                {
                    Straatnamen = pagedStreetNames,
                    Volgende = BuildNextUri(streetNameQuery.PaginationInfo, pagedStreetNames.Count, _responseOptions.Value.VolgendeUrl),
                    Pagination = streetNameQuery.PaginationInfo,
                    Sorting = streetNameQuery.Sorting
                };
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListItemV2 item, Language? taal)
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

        private static GeografischeNaam? GetHomoniemToevoegingByTaal(StreetNameListItemV2 item, Language? taal)
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

        protected static Uri? BuildNextUri(PaginationInfo paginationInfo, int itemsInCollection, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemsInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
