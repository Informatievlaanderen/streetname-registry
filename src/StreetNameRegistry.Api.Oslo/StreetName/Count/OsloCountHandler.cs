namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Syndication;
    using Query;

    public sealed class OsloCountHandler : OsloCountHandlerBase
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ConsumerPostalContext _postalContext;

        public OsloCountHandler(LegacyContext legacyContext, SyndicationContext syndicationContext, ConsumerPostalContext postalContext)
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
                    Aantal = request.Filtering.ShouldFilter
                        ? await new StreetNameListOsloQuery(_legacyContext, _syndicationContext, _postalContext)
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
