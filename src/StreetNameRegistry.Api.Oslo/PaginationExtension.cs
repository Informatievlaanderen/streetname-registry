namespace StreetNameRegistry.Api.Oslo
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;

    public static class PaginationExtension
    {
        public static Uri? BuildNextUri(
            this PaginationInfo paginationInfo,
            int itemCountInCollection,
            string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemCountInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
