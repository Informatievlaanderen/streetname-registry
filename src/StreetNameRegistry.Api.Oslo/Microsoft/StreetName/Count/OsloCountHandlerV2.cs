namespace StreetNameRegistry.Api.Oslo.Microsoft.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using global::Microsoft.EntityFrameworkCore;
    using Query;
    using StreetNameRegistry.Projections.Legacy.Microsoft;
    using StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameListV2;
    using StreetNameRegistry.Projections.Syndication.Microsoft;

    public sealed class OsloCountHandlerV2 : OsloCountHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;

        public OsloCountHandlerV2(LegacyContext legacyContext, SyndicationContext syndicationContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        public override async Task<TotaalAantalResponse> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListOsloQueryV2(_legacyContext, _syndicationContext)
                        .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                };
        }
    }
}
