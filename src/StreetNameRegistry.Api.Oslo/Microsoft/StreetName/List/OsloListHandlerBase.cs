namespace StreetNameRegistry.Api.Oslo.Microsoft.StreetName.List
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public sealed record OsloListRequest(FilteringHeader<StreetNameFilter> Filtering, SortingHeader Sorting, IPaginationRequest PaginationRequest) : IRequest<StreetNameListOsloResponse>;

    public abstract class OsloListHandlerBase : IRequestHandler<OsloListRequest, StreetNameListOsloResponse>
    {
        protected static Uri? BuildNextUri(PaginationInfo paginationInfo, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }

        public abstract Task<StreetNameListOsloResponse> Handle(OsloListRequest request, CancellationToken cancellationToken);
    }
}
