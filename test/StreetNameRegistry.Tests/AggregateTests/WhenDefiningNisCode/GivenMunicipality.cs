namespace StreetNameRegistry.Tests.AggregateTests.WhenDefiningNisCode
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

    public class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityNisCodeWasChanged()
        {
            var command = Fixture.Create<DefineMunicipalityNisCode>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityNisCodeWasChanged(command.MunicipalityId, command.NisCode))));
        }
    }

    public static class DefineMunicipalityNisCodeExtensions
    {
        public static DefineMunicipalityNisCode WithMunicipalityId(
            this DefineMunicipalityNisCode command,
            MunicipalityId municipalityId)
        {
            return new DefineMunicipalityNisCode(municipalityId, command.NisCode, command.Provenance);
        }
    }
}
