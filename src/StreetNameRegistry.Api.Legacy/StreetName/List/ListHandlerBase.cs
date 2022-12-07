namespace StreetNameRegistry.Api.Legacy.StreetName.List
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public sealed record ListRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting, IPaginationRequest Pagination) : IRequest<StreetNameListResponse>;

    public abstract class ListHandlerBase : IRequestHandler<ListRequest, StreetNameListResponse>
    {
        protected static Uri? BuildNextUri(PaginationInfo paginationInfo, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }

        public abstract Task<StreetNameListResponse> Handle(ListRequest request, CancellationToken cancellationToken);
    }
}
