namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingNameToClearedMunicipality
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityWasAlreadyNamed : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipalityWasAlreadyNamed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
        public void ThenNamedAgain(Language language)
        {
            var commandNameMunicipality = Fixture.Create<CorrectToClearedMunicipalityName>().WithLanguage(language);
            var municipalityWasNamed =
                new MunicipalityWasNamed(_municipalityId, new MunicipalityName("GreatName", language));

            municipalityWasNamed.SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(), municipalityWasNamed)
                .When(commandNameMunicipality)
                .Then(new Fact(_streamId, new MunicipalityWasNamed(_municipalityId, new MunicipalityName(string.Empty, language)))));
        }

        // No state check needed
    }

    public static class NameExtensions
    {
        public static CorrectToClearedMunicipalityName WithLanguage(this CorrectToClearedMunicipalityName command, Language language)
        {
            return new CorrectToClearedMunicipalityName(command.MunicipalityId, language, command.Provenance);
        }
    }
}
