namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;

    public sealed record OsloCountRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting) : IRequest<TotaalAantalResponse>;

    public sealed class ElasticOsloCountHandler: IRequestHandler<OsloCountRequest, TotaalAantalResponse>
    {
        private readonly IStreetNameApiElasticSearchClient _streetNameApiElasticSearchClient;

        public ElasticOsloCountHandler(IStreetNameApiElasticSearchClient streetNameApiElasticSearchClient)
        {
            _streetNameApiElasticSearchClient = streetNameApiElasticSearchClient;
        }

        public async Task<TotaalAantalResponse> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.Filtering;

            var addressCountResult = await _streetNameApiElasticSearchClient.CountStreetNames(
                filtering.Filter?.StreetNameName,
                filtering.Filter?.NisCode,
                filtering.Filter?.MunicipalityName,
                filtering.Filter?.Status,
                filtering.Filter?.IsInFlemishRegion);

            return new TotaalAantalResponse
            {
                Aantal = (int)addressCountResult
            };
        }
    }
}
