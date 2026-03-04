namespace StreetNameRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Projections.Feed;
    using Projections.Feed.StreetNameFeed;
    using Xunit;

    public sealed class StreetNameFeedExtensionsTests
    {
        private const int TestMaxPageSize = 5;

        [Fact]
        public async Task WhenNoItemsExist_ThenReturnsPage1()
        {
            // Arrange
            await using var context = CreateContext();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(1);
        }

        [Fact]
        public async Task WhenPageIsNotFull_ThenReturnsSamePage()
        {
            // Arrange
            await using var context = CreateContext();
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 1, persistentLocalId: 1));
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 2, persistentLocalId: 2));
            await context.SaveChangesAsync();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(1, "page 1 has 2 items and max page size is 5");
        }

        [Fact]
        public async Task WhenPageIsFull_ThenReturnsNextPage()
        {
            // Arrange
            await using var context = CreateContext();
            for (var i = 1; i <= TestMaxPageSize; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }
            await context.SaveChangesAsync();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(2, "page 1 is full with 5 items");
        }

        [Fact]
        public async Task WhenPageIsAlmostFull_ThenReturnsSamePage()
        {
            // Arrange
            await using var context = CreateContext();
            for (var i = 1; i < TestMaxPageSize; i++) // One less than max
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }
            await context.SaveChangesAsync();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(1, "page 1 has 4 items and max page size is 5");
        }

        [Fact]
        public async Task WhenMultiplePagesExist_ThenReturnsMaxPage()
        {
            // Arrange
            await using var context = CreateContext();

            // Page 1 is full
            for (var i = 1; i <= TestMaxPageSize; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }

            // Page 2 has some items
            context.StreetNameFeed.Add(CreateFeedItem(page: 2, position: 6, persistentLocalId: 6));
            context.StreetNameFeed.Add(CreateFeedItem(page: 2, position: 7, persistentLocalId: 7));
            await context.SaveChangesAsync();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(2, "page 2 exists and is not full");
        }

        [Fact]
        public async Task WhenMultiplePagesExistAndLastPageIsFull_ThenReturnsNextPage()
        {
            // Arrange
            await using var context = CreateContext();

            // Page 1 is full
            for (var i = 1; i <= TestMaxPageSize; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }

            // Page 2 is also full
            for (var i = 1; i <= TestMaxPageSize; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 2, position: TestMaxPageSize + i, persistentLocalId: TestMaxPageSize + i));
            }
            await context.SaveChangesAsync();

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(3, "page 2 is full so we need page 3");
        }

        [Fact]
        public async Task WhenUnsavedItemsExistOnCurrentPage_ThenCountsThemTowardsPageSize()
        {
            // Arrange
            await using var context = CreateContext();

            // Add 3 items to page 1 and save
            for (var i = 1; i <= 3; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }
            await context.SaveChangesAsync();

            // Add 2 more unsaved items to page 1 (total will be 5 = full)
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 4, persistentLocalId: 4));
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 5, persistentLocalId: 5));
            // Don't save - these are local/pending

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(2, "page 1 has 3 saved + 2 unsaved = 5 items (full)");
        }

        [Fact]
        public async Task WhenUnsavedItemsExistButPageNotFull_ThenReturnsSamePage()
        {
            // Arrange
            await using var context = CreateContext();

            // Add 2 items to page 1 and save
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 1, persistentLocalId: 1));
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 2, persistentLocalId: 2));
            await context.SaveChangesAsync();

            // Add 1 more unsaved item to page 1 (total will be 3, not full)
            context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: 3, persistentLocalId: 3));
            // Don't save

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(1, "page 1 has 2 saved + 1 unsaved = 3 items (not full)");
        }

        [Fact]
        public async Task WhenUnsavedItemsExistOnDifferentPage_ThenDoesNotCountThem()
        {
            // Arrange
            await using var context = CreateContext();

            // Add 4 items to page 1 and save
            for (var i = 1; i <= 4; i++)
            {
                context.StreetNameFeed.Add(CreateFeedItem(page: 1, position: i, persistentLocalId: i));
            }
            await context.SaveChangesAsync();

            // Add unsaved item to page 2 (should not affect page 1 count)
            context.StreetNameFeed.Add(CreateFeedItem(page: 2, position: 5, persistentLocalId: 5));
            // Don't save

            // Act
            var page = await context.CalculatePage(TestMaxPageSize);

            // Assert
            page.Should().Be(1, "page 1 has 4 saved items and unsaved item is on page 2, so page 1 is not full");
        }

        private static StreetNameFeedItem CreateFeedItem(int page, long position, int persistentLocalId)
        {
            return new StreetNameFeedItem(position, page, persistentLocalId)
            {
                CloudEventAsString = "{}"
            };
        }

        private static FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}

