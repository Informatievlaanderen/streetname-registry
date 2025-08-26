namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Converters;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using NodaTime.Extensions;
    using Projections.Legacy;
    using Query;

    public sealed record OsloListRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting, IPaginationRequest PaginationRequest) : IRequest<StreetNameListOsloResponse>;

    public sealed class OsloListHandlerV2 : IRequestHandler<OsloListRequest, StreetNameListOsloResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public OsloListHandlerV2(
            LegacyContext legacyContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _responseOptions = responseOptions;
        }

        public async Task<StreetNameListOsloResponse> Handle(OsloListRequest request, CancellationToken cancellationToken)
        {
            var streetNameQuery = new StreetNameListOsloQueryV2(_legacyContext)
                    .Fetch<StreetNameListView, StreetNameListViewQueryResponse>(request.Filtering, request.Sorting, request.PaginationRequest);

            var pagedStreetNames = await streetNameQuery
                .Items
                .Select(m => new StreetNameListOsloItemResponse(
                    m.StreetNamePersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    GetGeografischeNaamByTaal(m, m.PrimaryLanguage),
                    GetHomoniemToevoegingByTaal(m, m.PrimaryLanguage),
                    m.Status.ConvertFromMunicipalityStreetNameStatus(),
                    m.Version.ToInstant().ToBelgianDateTimeOffset()))
                .ToListAsync(cancellationToken);

            return
                new StreetNameListOsloResponse
                {
                    Straatnamen = pagedStreetNames,
                    Volgende = BuildNextUri(streetNameQuery.PaginationInfo, pagedStreetNames.Count, _responseOptions.Value.VolgendeUrl),
                    Context = _responseOptions.Value.ContextUrlList,
                    Sorting = streetNameQuery.Sorting,
                    Pagination = streetNameQuery.PaginationInfo
                };
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListViewQueryResponse item, Language? taal)
        {
            switch (taal)
            {
                case null when !string.IsNullOrEmpty(item.StreetNameDutch):
                case Language.Dutch when !string.IsNullOrEmpty(item.StreetNameDutch):
                    return new GeografischeNaam(
                        item.StreetNameDutch,
                        Taal.NL);

                case Language.French when !string.IsNullOrEmpty(item.StreetNameFrench):
                    return new GeografischeNaam(
                        item.StreetNameFrench,
                        Taal.FR);

                case Language.German when !string.IsNullOrEmpty(item.StreetNameGerman):
                    return new GeografischeNaam(
                        item.StreetNameGerman,
                        Taal.DE);

                case Language.English when !string.IsNullOrEmpty(item.StreetNameEnglish):
                    return new GeografischeNaam(
                        item.StreetNameEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }

        private static GeografischeNaam? GetHomoniemToevoegingByTaal(StreetNameListViewQueryResponse item, Language? taal)
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

        private static Uri? BuildNextUri(PaginationInfo paginationInfo, int itemsInCollection, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemsInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
