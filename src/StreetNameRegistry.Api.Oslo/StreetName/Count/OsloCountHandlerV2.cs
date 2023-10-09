namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Query;

    public sealed class OsloCountHandlerV2 : OsloCountHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        public OsloCountHandlerV2(LegacyContext legacyContext, SyndicationContext syndicationContext, ConsumerPostalContext postalContext)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _postalContext = postalContext;
        }

        public override async Task<TotaalAantalResponse> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListOsloQueryV2(_legacyContext, _syndicationContext, _postalContext)
                        .Fetch<StreetNameListItemV2, StreetNameListItemV2>(request.Filtering, request.Sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                };
        }
    }
}
