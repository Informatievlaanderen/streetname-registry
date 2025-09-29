namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Converters;
    using MediatR;
    using Microsoft.Extensions.Options;
    using NodaTime.Extensions;
    using Projections.Elastic.StreetNameList;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;

    public sealed class ElasticOsloListHandler : IRequestHandler<OsloListRequest, StreetNameListOsloResponse>

    {
        private readonly IStreetNameApiElasticSearchClient _streetNameApiElasticSearchClient;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ElasticOsloListHandler(
            IStreetNameApiElasticSearchClient streetNameApiElasticSearchClient,
            IOptions<ResponseOptions> responseOptions)
        {
            _streetNameApiElasticSearchClient = streetNameApiElasticSearchClient;
            _responseOptions = responseOptions;
        }


        public async Task<StreetNameListOsloResponse> Handle(OsloListRequest request, CancellationToken cancellationToken)
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
                .Select(s => new StreetNameListOsloItemResponse(
                    s.StreetNamePersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    GetGeografischeNaamByTaal(s, s.Municipality.PrimaryLanguage),
                    GetHomoniemToevoegingByTaal(s, s.Municipality.PrimaryLanguage),
                    s.Status.ConvertFromMunicipalityStreetNameStatus(),
                    s.VersionTimestamp.ToInstant().ToBelgianDateTimeOffset()))
                .ToList();

            var paginationInfo = new PaginationInfo(pagination.Offset, pagination.Limit, pagination.Limit > 0);
            return
                new StreetNameListOsloResponse
                {
                    Straatnamen = streetNames,
                    Volgende = paginationInfo.BuildNextUri(streetNames.Count, _responseOptions.Value.VolgendeUrl),
                    Context = _responseOptions.Value.ContextUrlList,
                    Sorting = request.Sorting,
                    Pagination = paginationInfo
                };
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListDocument item, Language? taal)
        {
            var name = item.Names.SingleOrDefault(x => x.Language == taal);

            switch (name?.Language)
            {
                case null when !string.IsNullOrEmpty(item.Names.SingleOrDefault(x => x.Language == Language.nl)?.Spelling):
                case Language.nl when !string.IsNullOrEmpty(name.Spelling):
                    return new GeografischeNaam(
                        item.Names.Single(x => x.Language == Language.nl).Spelling,
                        Taal.NL);

                case Language.fr when !string.IsNullOrEmpty(name.Spelling):
                    return new GeografischeNaam(
                        name.Spelling,
                        Taal.FR);

                case Language.de when !string.IsNullOrEmpty(name.Spelling):
                    return new GeografischeNaam(
                        name.Spelling,
                        Taal.DE);

                case Language.en when !string.IsNullOrEmpty(name.Spelling):
                    return new GeografischeNaam(
                        name.Spelling,
                        Taal.EN);

                default:
                    return null;
            }
        }

        private static GeografischeNaam? GetHomoniemToevoegingByTaal(StreetNameListDocument item, Language? taal)
        {
            var homonym = item.HomonymAdditions.SingleOrDefault(x => x.Language == taal);

            switch (homonym?.Language)
            {
                case null when !string.IsNullOrEmpty(item.HomonymAdditions.SingleOrDefault(x => x.Language == Language.nl)?.Spelling):
                case Language.nl when !string.IsNullOrEmpty(homonym.Spelling):
                    return new GeografischeNaam(
                        item.HomonymAdditions.Single(x => x.Language == Language.nl).Spelling,
                        Taal.NL);

                case Language.fr when !string.IsNullOrEmpty(homonym.Spelling):
                    return new GeografischeNaam(
                        homonym.Spelling,
                        Taal.FR);

                case Language.de when !string.IsNullOrEmpty(homonym.Spelling):
                    return new GeografischeNaam(
                        homonym.Spelling,
                        Taal.DE);

                case Language.en when !string.IsNullOrEmpty(homonym.Spelling):
                    return new GeografischeNaam(
                        homonym.Spelling,
                        Taal.EN);

                default:
                    return null;
            }
        }
    }
}
