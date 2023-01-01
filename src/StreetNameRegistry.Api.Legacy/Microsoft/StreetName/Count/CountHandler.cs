namespace StreetNameRegistry.Api.Legacy.Microsoft.StreetName.Count
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using global::Microsoft.EntityFrameworkCore;
    using Query;
    using StreetNameRegistry.Projections.Legacy.Microsoft;
    using StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameList;
    using StreetNameRegistry.Projections.Syndication.Microsoft;

    public sealed class CountHandler : CountHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;

        public CountHandler(LegacyContext legacyContext, SyndicationContext syndicationContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        public override async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
                {
                    Aantal = request.Filtering.ShouldFilter
                        ? await new StreetNameListQuery(_legacyContext, _syndicationContext)
                            .Fetch<StreetNameListItem, StreetNameListItem>(request.Filtering, request.Sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32((await _legacyContext
                                .StreetNameListViewCount
                                .FirstAsync(cancellationToken: cancellationToken))
                            .Count)
                };
        }
    }
}
