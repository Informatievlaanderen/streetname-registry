namespace StreetNameRegistry.Api.Oslo.Handlers.Count
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.StreetName.Query;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.StreetNameList;

    public class OsloCountHandler : OsloCountHandlerBase
    {
        public override async Task<IActionResult> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new OkObjectResult(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new StreetNameListOsloQuery(request.LegacyContext, request.SyndicationContext)
                            .Fetch<StreetNameListItem, StreetNameListItem>(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32((await request.LegacyContext
                                .StreetNameListViewCount
                                .FirstAsync(cancellationToken: cancellationToken))
                            .Count)
                });

        }
    }
}
