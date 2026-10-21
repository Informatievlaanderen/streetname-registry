namespace StreetNameRegistry.Tests.Bosa
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using Projections.Bosa;
    using Projections.Bosa.Infrastructure;
    using Xunit;

    public class BosaContextTests
    {
        private static readonly XNamespace Mutations = StreetNameMutationXml.Mutations;

        // Days start at 20:00 UTC, 2026-10-21 runs from 2026-10-20T20:00Z.
        private static readonly BosaDay BosaDay = new BosaDay(new BosaOptions { ChangesFrom = "2026-10-20T20:00:00Z" });
        private static readonly Instant Day1 = Instant.FromUtc(2026, 10, 21, 8, 0);
        private static readonly Instant Day2 = Instant.FromUtc(2026, 10, 22, 8, 0);

        private readonly string _databaseName = Guid.NewGuid().ToString();
        private readonly InMemoryBosaFileStore _fileStore = new InMemoryBosaFileStore();
        private readonly BosaDailyFilePublisher _publisher;

        public BosaContextTests()
        {
            _publisher = new BosaDailyFilePublisher(
                new BosaXmlWriter(),
                _fileStore,
                Options.Create(new BosaOptions { FileNameFormat = "{0:yyyyMMdd}-{1}.xml" }));
        }

        [Fact]
        public async Task WhenSavingChanges_ThenAddAndUpdateFilesAreUploaded()
        {
            await using (var context = CreateContext())
            {
                await context.CreateStreetName(BosaDay, StreetName(1), Day1, "created", 1, null, CancellationToken.None);
                await context.SaveChangesAsync();
            }

            File("20261021-add.xml").Root!.Elements(Mutations + "addStreetNameVersion").Should().ContainSingle();
            File("20261021-update.xml").Root!.Name.Should().Be(Mutations + "update");
            File("20261021-update.xml").Root!.Elements().Should().BeEmpty();

            var add = Encoding.UTF8.GetString(_fileStore.Files["20261021-add.xml"]);
            add.Should().Contain("<tns:addStreetNameVersion>").And.Contain("<com:code>");
        }

        [Fact]
        public async Task WhenChangedInALaterBatch_ThenFilesAreRebuilt()
        {
            await using (var context = CreateContext())
            {
                await context.CreateStreetName(BosaDay, StreetName(1), Day1, "created", 1, null, CancellationToken.None);
                await context.SaveChangesAsync();
            }

            await using (var context = CreateContext())
            {
                await context.CreateStreetName(BosaDay, StreetName(2), Day1, "created", 2, null, CancellationToken.None);
                await context.SaveChangesAsync();
            }

            File("20261021-add.xml").Root!.Elements().Should().HaveCount(2);
        }

        [Fact]
        public async Task WhenBatchSpansTwoDays_ThenFilesOfBothDaysAreUploaded()
        {
            await using (var context = CreateContext())
            {
                await context.CreateStreetName(BosaDay, StreetName(1), Day1, "created", 1, null, CancellationToken.None);
                await context.ChangeStreetName(BosaDay, 1, StreetNameChangeType.NewVersion, _ => { }, Day2, "changed", 2, CancellationToken.None);
                await context.SaveChangesAsync();
            }

            _fileStore.Files.Keys.Should().BeEquivalentTo(
                "20261021-add.xml", "20261021-update.xml",
                "20261022-add.xml", "20261022-update.xml");
        }

        [Fact]
        public async Task WhenNoStreetNameChanges_ThenNothingIsUploaded()
        {
            await using (var context = CreateContext())
            {
                await context.UpdateProjectionState("projection", 10, CancellationToken.None);
                await context.SaveChangesAsync();
            }

            _fileStore.Files.Should().BeEmpty();
        }

        private static StreetNameBosaItem StreetName(int persistentLocalId) => new StreetNameBosaItem
        {
            PersistentLocalId = persistentLocalId,
            Namespace = "https://data.vlaanderen.be/id/straatnaam",
            NameDutch = "Straat",
            NisCode = "11001"
        };

        private BosaContext CreateContext()
            => new BosaContext(
                new DbContextOptionsBuilder<BosaContext>().UseInMemoryDatabase(_databaseName).Options,
                _publisher);

        private XDocument File(string name)
        {
            _fileStore.Files.Should().ContainKey(name);
            return XDocument.Parse(Encoding.UTF8.GetString(_fileStore.Files[name]));
        }
    }
}
