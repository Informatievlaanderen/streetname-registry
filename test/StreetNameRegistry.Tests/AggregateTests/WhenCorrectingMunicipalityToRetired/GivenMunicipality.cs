namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingMunicipalityToRetired
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
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityWasCorrectedToRetired()
        {
            var commandCorrectMunicipality = Fixture.Create<CorrectToRetiredMunicipality>();
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>())
                .When(commandCorrectMunicipality)
                .Then(new Fact(_streamId, new MunicipalityWasCorrectedToRetired(_municipalityId))));
        }

        [Fact]
        public void WithRetiredMunicipality_ThenNone()
        {
            var commandCorrectMunicipality = Fixture.Create<CorrectToRetiredMunicipality>();
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityWasRetired>())
                .When(commandCorrectMunicipality)
                .ThenNone());
        }
    }
}
