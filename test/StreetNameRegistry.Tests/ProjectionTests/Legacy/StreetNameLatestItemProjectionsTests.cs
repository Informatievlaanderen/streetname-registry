namespace StreetNameRegistry.Tests.ProjectionTests.Legacy
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using StreetNameRegistry.Projections.Legacy.StreetNameDetail;
    using StreetNameRegistry.StreetName;
    using StreetNameRegistry.Tests.Generate;
    using StreetNameRegistry.Tests.Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class StreetNameLatestItemProjectionsTests : StreetNameRegistryProjectionTest<StreetNameDetailProjections>
    {
        public StreetNameLatestItemProjectionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StreetNameBecameCompleteSetsCompleted()
        {
            var id = Arrange(Produce.Guid());
            var provenance = Arrange(Generate.Provenance);

            await GivenEvents(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNameBecameComplete
                    .Select(e => e.WithId(id).WithProvenance(provenance)))
                .Then(async ct => {
                    var entity = await ct.FindAsync<StreetNameDetail>(id);
                    entity.Should().NotBeNull();
                    entity.Complete.Should().BeTrue();
                });
        }

        [Fact]
        public async Task StreetNameWasRemovedRemovesRecord()
        {
            var id = Arrange(Produce.Guid());
            var provenance = Arrange(Generate.Provenance);

            await GivenEvents(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNameWasRemoved
                    .Select(e => e.WithId(id).WithProvenance(provenance)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameDetail>(id);
                    entity.Should().NotBeNull();
                    entity.Removed.Should().BeTrue();
                });
        }

        [Fact]
        public async Task StreetNameWasRetiredSetsStatus()
        {
            var id = Arrange(Produce.Guid());
            var provenance = Arrange(Generate.Provenance);

            await GivenEvents(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNameWasRetired
                    .Select(e => e.WithId(id).WithProvenance(provenance)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameDetail>(id);
                    entity.Should().NotBeNull();
                    entity.Status.Should().Be(StreetNameStatus.Retired);
                });
        }

        [Fact]
        public async Task StreetNamePersistentLocalIdWasAssignedAssignsOsloId()
        {
            var id = Arrange(Produce.Guid());
            var persistentLocalId = Arrange(Produce.Integer(10000, 15000));

            await GivenEvents(Generate.EventsFor.StreetName(id))
                .Project(Generate.StreetNamePersistentLocalIdWasAssigned
                    .Select(e => e.WithId(id)
                        .WithPersistentLocalId(persistentLocalId)))
                .Then(async ct =>
                {
                    var entity = await ct.FindAsync<StreetNameDetail>(id);
                    entity.Should().NotBeNull();
                    entity.PersistentLocalId.Should().Be(persistentLocalId);
                });
        }
    }
}
