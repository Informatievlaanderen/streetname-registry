namespace StreetNameRegistry.Projections.Feed.StreetNameFeed
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Microsoft.EntityFrameworkCore;

    public static class StreetNameFeedExtensions
    {
        public static async Task<int> CalculatePage(this FeedContext context, int maxPageSize = ChangeFeedService.DefaultMaxPageSize)
        {
            if (!await context.StreetNameFeed.AnyAsync())
            {
                return 1;
            }

            var maxPage = await context.StreetNameFeed.MaxAsync(x => x.Page);
            var dbCount = await context.StreetNameFeed.CountAsync(x => x.Page == maxPage);

            // Count pending (unsaved) items in the change tracker assigned to the current max page
            // This fixes the issue where multiple items added in the same batch would all get the same page
            var localCount = context.StreetNameFeed.Local
                .Count(x => x.Page == maxPage && context.Entry(x).State == EntityState.Added);

            var totalCount = dbCount + localCount;

            return totalCount >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}
