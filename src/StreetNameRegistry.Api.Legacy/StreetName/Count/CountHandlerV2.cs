namespace StreetNameRegistry.Api.Legacy.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Query;

    public sealed record CountRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting) : IRequest<TotaalAantalResponse>;

    public sealed class CountHandlerV2 : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        public CountHandlerV2(LegacyContext legacyContext, SyndicationContext syndicationContext, ConsumerPostalContext postalContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _postalContext = postalContext;
        }

        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListQueryV2(_legacyContext, _syndicationContext, _postalContext)
                        .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                };
        }
    }
}
