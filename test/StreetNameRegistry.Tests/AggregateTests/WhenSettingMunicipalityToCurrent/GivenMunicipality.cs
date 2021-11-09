namespace StreetNameRegistry.Tests.AggregateTests.WhenSettingMunicipalityToCurrent
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using StreetName.Commands;
    using StreetName.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
        }

        [Fact]
        public void ThenMunicipalityBecameCurrent()
        {
            var command = Fixture.Create<SetMunicipalityToCurrent>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_municipalityId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new[]
                {
                    new Fact(_municipalityId, new MunicipalityBecameCurrent(command.MunicipalityId))
                }));
        }

        [Fact]
        public void WhenMunicipalityAlreadyBecameCurrentThenNothingHappens()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var command = Fixture.Create<SetMunicipalityToCurrent>()
                .WithMunicipalityId(new MunicipalityId(_municipalityId));

            Assert(new Scenario()
                .Given(
                    _municipalityId,
                    municipalityWasImported,
                    municipalityBecameCurrent
                )
                .When(command)
                .ThenNone());
        }
    }

    public static class SetMunicipalityToCurrentExtensions
    {
        public static SetMunicipalityToCurrent WithMunicipalityId(
            this SetMunicipalityToCurrent command,
            MunicipalityId municipalityId)
        {
            return new SetMunicipalityToCurrent(municipalityId, command.Provenance);
        }
    }
}