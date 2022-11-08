namespace StreetNameRegistry.Api.Oslo.Handlers.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.StreetNameListV2;
    using StreetNameRegistry.Api.Oslo.Abstractions.StreetName.Query;

    public class OsloCountHandlerV2 : OsloCountHandlerBase
    {
        public override async Task<IActionResult> Handle(OsloCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new OkObjectResult(
                new TotaalAantalResponse
                {
                    Aantal = await new StreetNameListOsloQueryV2(request.LegacyContext, request.SyndicationContext)
                        .Fetch<StreetNameListItemV2, StreetNameListItemV2>(filtering, sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                });
        }
    }
}
