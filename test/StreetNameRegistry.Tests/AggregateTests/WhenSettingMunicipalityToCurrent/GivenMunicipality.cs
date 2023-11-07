namespace StreetNameRegistry.Tests.AggregateTests.WhenSettingMunicipalityToCurrent
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityBecameCurrent()
        {
            var command = Fixture.Create<SetMunicipalityToCurrent>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityBecameCurrent(command.MunicipalityId))));
        }

        [Fact]
        public void WithCurrentMunicipality_ThenNone()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var command = Fixture.Create<SetMunicipalityToCurrent>();

            Assert(new Scenario()
                .Given(
                    _streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent
                )
                .When(command)
                .ThenNone());
        }
    }
}
