namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingStreetName
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : StreetNameRegistryTest
    {
        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
        }

        [Fact]
        public void ThenStreetNameWasRemovedV2()
        {
            var command = Fixture.Create<RemoveStreetName>();
            var municipalityStreamId = new MunicipalityStreamId(Fixture.Create<MunicipalityId>());

            // Act, assert
            Assert(new Scenario()
                .Given(municipalityStreamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<StreetNameWasProposedV2>())
                .When(command)
                .Then(new Fact(municipalityStreamId, new StreetNameWasRemovedV2(command.MunicipalityId,
                   command.PersistentLocalId))));
        }

        [Fact]
        public void WithRemovedStreetName_ThenNone()
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
