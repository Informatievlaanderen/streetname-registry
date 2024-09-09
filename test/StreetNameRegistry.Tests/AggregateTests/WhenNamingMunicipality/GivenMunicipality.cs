namespace StreetNameRegistry.Tests.AggregateTests.WhenNamingMunicipality
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

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.English)]
        [InlineData(Language.German)]
        public void ThenMunicipalityGetsNamed(Language language)
        {
            var commandNameMunicipality = Fixture.Create<NameMunicipality>().WithName("GreatName", language);
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>())
                .When(commandNameMunicipality)
                .Then(new Fact(_streamId, new MunicipalityWasNamed(_municipalityId, new MunicipalityName("GreatName", language)))));
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.English)]
        [InlineData(Language.German)]
        public void WithTheSameMunicipalityName_ThenMunicipalityWasNamed(Language language)
        {
            var commandNameMunicipality = Fixture.Create<NameMunicipality>().WithName("GreatName", language);
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>(), Fixture.Create<MunicipalityWasNamed>())
                .When(commandNameMunicipality)
                .Then(new Fact(_streamId, new MunicipalityWasNamed(_municipalityId, new MunicipalityName("GreatName", language)))));
        }

        // No state check needed
    }
    public static class NameExtensions
    {
        public static NameMunicipality WithName(this NameMunicipality command, string name, Language language)
        {
            return new NameMunicipality(command.MunicipalityId, new MunicipalityName(name, language), command.Provenance);
        }
    }

}
