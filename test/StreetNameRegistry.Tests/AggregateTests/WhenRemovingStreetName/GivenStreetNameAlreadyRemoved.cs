namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingStreetName
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Municipality.Commands;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class GivenStreetNameAlreadyRemoved : StreetNameRegistryTest
    {
        public GivenStreetNameAlreadyRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
        }

        [Fact]
        public void ThenDoNone()
        {
            var command = Fixture.Create<RemoveStreetName>();

            // Act, assert
            Assert(new Scenario()
                .Given(new MunicipalityStreamId(Fixture.Create<MunicipalityId>()),
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasRemovedV2>())
                .When(command)
                .ThenNone());
        }
    }
}
