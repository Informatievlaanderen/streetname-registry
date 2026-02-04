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
            if (!context.StreetNameFeed.Any())
                return 1;

            var maxPage = await context.StreetNameFeed.MaxAsync(x => x.Page);

            var pageItems = await context.StreetNameFeed.CountAsync(x => x.Page == maxPage);
            return pageItems >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}
