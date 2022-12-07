namespace StreetNameRegistry.Api.Legacy.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Query;

    public sealed class CountHandlerV2 : CountHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;

        public CountHandlerV2(LegacyContext legacyContext, SyndicationContext syndicationContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        public override async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListQueryV2(_legacyContext, _syndicationContext)
                        .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                };
        }
    }
}
