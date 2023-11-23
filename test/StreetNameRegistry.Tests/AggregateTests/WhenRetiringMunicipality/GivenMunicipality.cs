namespace StreetNameRegistry.Tests.AggregateTests.WhenRetiringMunicipality
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

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityWasRetired()
        {
            var command = Fixture.Create<RetireMunicipality>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityWasRetired(command.MunicipalityId))));
        }

        [Fact]
        public void WithRetiredMunicipality_ThenNone()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityWasRetired = Fixture.Create<MunicipalityWasRetired>();
            var command = Fixture.Create<RetireMunicipality>()
                .WithMunicipalityId(new MunicipalityId(_municipalityId));

            Assert(new Scenario()
                .Given(
                    _streamId,
                    municipalityWasImported,
                    municipalityWasRetired
                )
                .When(command)
                .ThenNone());
        }
    }

    public static class RetireMunicipalityExtensions
    {
        public static RetireMunicipality WithMunicipalityId(
            this RetireMunicipality command,
            MunicipalityId municipalityId)
        {
            return new RetireMunicipality(municipalityId, new RetirementDate(command.RetirementDate), command.Provenance);
        }
    }
}
