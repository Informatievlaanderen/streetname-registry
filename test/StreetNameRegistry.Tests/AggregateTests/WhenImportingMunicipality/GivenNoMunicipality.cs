namespace StreetNameRegistry.Tests.AggregateTests.WhenImportingMunicipality
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

    public sealed class GivenNoMunicipality : StreetNameRegistryTest
    {
        public GivenNoMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenMunicipalityWasImported()
        {
            var command = Fixture.Create<ImportMunicipality>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new MunicipalityStreamId(command.MunicipalityId),
                    new MunicipalityWasImported(command.MunicipalityId, command.NisCode))));
        }
    }
}
