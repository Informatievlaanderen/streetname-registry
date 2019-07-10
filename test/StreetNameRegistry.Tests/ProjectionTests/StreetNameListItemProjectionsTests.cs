namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Generate;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public class StreetNameListItemProjectionsTests : ProjectionTest<LegacyContext, StreetNameListProjections>
    {
        public StreetNameListItemProjectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StreetNameWasRegisteredInsertsRecord()
        {
            var id = Arrange(Produce.Guid());
            var provenance = Arrange(Generate.Provenance);

            await Given()
                .Project(Generate.StreetNameWasRegistered
                    .Select(e => e.WithId(id)
                            .WithProvenance(provenance)))
                .Then(async ct => (await ct.FindAsync<StreetNameListItem>(id)).Should().NotBeNull());
        }

        [Fact]
        public async Task StreetNameNameWasNamedChangesName()
        {
            var id = Arrange(Produce.Guid());
            var name = Arrange(Generate.StreetNameString);
            var provenance = Arrange(Generate.Provenance);
            var language = Language.Dutch;

            await Given(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNameNameWasNamed
                    .Select(e => e.WithId(id)
                        .WithName(name)
                        .WithLanguage(language)
                        .WithProvenance(provenance)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameListItem>(id);
                    entity.Should().NotBeNull();
                    entity.NameDutch.Should().Be(name);
                });
        }

        [Fact]
        public async Task StreetNameNameWasClearedClearsName()
        {
            var id = Arrange(Produce.Guid());
            var nameDutch = Arrange(Generate.StreetNameString);
            var provenance = Arrange(Generate.Provenance);
            var language = Language.Dutch;

            await Given(Generate.EventsFor.StreetName(id, nameDutch))
                .Project(Generate.StreetNameNameWasCleared
                    .Select(e => e.WithId(id)
                        .WithLanguage(language)
                        .WithProvenance(provenance)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameListItem>(id);
                    entity.Should().NotBeNull();
                    entity.NameDutch.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task StreetNamePersistentLocalIdWasAssignedAssignsOsloId()
        {
            var id = Arrange(Produce.Guid());
            var persistentLocalId = Arrange(Produce.Integer(10000, 15000));

            await Given(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNamePersistentLocalIdWasAssigned
                    .Select(e => e.WithId(id)
                    .WithPersistentLocalId(persistentLocalId)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameListItem>(id);
                    entity.Should().NotBeNull();
                    entity.PersistentLocalId.Should().Be(persistentLocalId);
                });
        }

        [Fact]
        public async Task EventThrowsIfNoRecordFound()
        {
            var id = Arrange(Produce.Guid());

            try
            {
                await Given()
                    .Project(Generate.StreetNamePersistentLocalIdWasAssigned.Select(e => e.WithId(id)))
                    .Then(async ct =>
                    {
                        var entity = await ct.FindAsync<StreetNameListItem>(id);
                        entity.Should().BeNull();
                    });
            }
            catch (Exception e)
            {
                Assert.IsType<ProjectionItemNotFoundException<StreetNameListProjections>>(e);
            }
        }

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> options)
        {
            return new LegacyContext(options);
        }
    }
}
