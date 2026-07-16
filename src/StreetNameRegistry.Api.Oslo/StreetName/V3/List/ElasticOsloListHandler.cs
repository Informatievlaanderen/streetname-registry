namespace StreetNameRegistry.Api.Oslo.StreetName.V3.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Converters;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using NodaTime.Extensions;
    using Projections.Elastic.StreetNameList;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;

    public sealed record OsloListRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting, IPaginationRequest PaginationRequest) : IRequest<StreetNameListOsloV3Response>;

    public sealed class ElasticOsloListHandler : IRequestHandler<OsloListRequest, StreetNameListOsloV3Response>
    {
        private readonly IStreetNameApiElasticSearchClient _streetNameApiElasticSearchClient;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public ElasticOsloListHandler(
            IStreetNameApiElasticSearchClient streetNameApiElasticSearchClient,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _streetNameApiElasticSearchClient = streetNameApiElasticSearchClient;
            _responseOptions = responseOptions;
        }


        public async Task<StreetNameListOsloV3Response> Handle(OsloListRequest request, CancellationToken cancellationToken)
        {
            var pagination = (PaginationRequest)request.PaginationRequest;
            var filtering = request.Filtering;

            var streetNameListResult = await _streetNameApiElasticSearchClient.ListStreetNames(
                filtering.Filter?.StreetNameName,
                filtering.Filter?.NisCode,
                filtering.Filter?.MunicipalityName,
                filtering.Filter?.Status,
                filtering.Filter?.IsInFlemishRegion,
                from: pagination.Offset,
                size: pagination.Limit);

            var streetNames = streetNameListResult.StreetNames
                .Select(s => new StreetNameListItemOsloV3Response(
                    s.StreetNamePersistentLocalId,
                    _responseOptions.Value.DetailUrl,
                    GetGeografischeNamenByTaal(s),
                    GetHomoniemToevoegingByTaal(s),
                    s.Status.ConvertOsloFromMunicipalityStreetNameStatus(),
                    s.VersionTimestamp.ToInstant().ToBelgianDateTimeOffset()))
                .ToList();

            var paginationInfo = new PaginationInfo(pagination.Offset, pagination.Limit, pagination.Limit > 0);
            return
                new StreetNameListOsloV3Response
                {
                    Straatnamen = streetNames,
                    Volgende = paginationInfo.BuildNextUri(streetNames.Count, _responseOptions.Value.VolgendeUrl),
                    Context = _responseOptions.Value.ContextUrlList,
                    Sorting = request.Sorting,
                    Pagination = paginationInfo
                };
        }

        private static Taal MapElasticLanguage(Language language)
        {
            switch (language)
            {
                case Language.nl:
                    return Taal.Nl;
                case Language.en:
                    return Taal.En;
                case Language.fr:
                    return Taal.Fr;
                case Language.de:
                    return Taal.De;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
        }

        private static IEnumerable<GeografischeNaam> GetGeografischeNamenByTaal(StreetNameListDocument item)
        {
            return item
                .Names
                .Select(name => new GeografischeNaam(name.Spelling, MapElasticLanguage(name.Language)));
        }

        private static IEnumerable<GeografischeNaam>? GetHomoniemToevoegingByTaal(StreetNameListDocument item)
        {
            if(!item.HomonymAdditions.Any())
                return null;

            return item
                .HomonymAdditions
                .Select(name => new GeografischeNaam(name.Spelling, MapElasticLanguage(name.Language)));
        }
    }
}
