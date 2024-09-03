namespace StreetNameRegistry.Tests.AggregateTests.WhenRequestingCreateOsloSnapshots
{
    using AllStream;
    using AllStream.Commands;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelsExist : StreetNameRegistryTest
    {
        public GivenParcelsExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenParcelOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [new PersistentLocalId(1)],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(AllStreamId.Instance)
                .When(command)
                .Then(AllStreamId.Instance,
                    new StreetNameOsloSnapshotsWereRequested(
                        command.PersistentLocalIds)));
        }
    }
}
