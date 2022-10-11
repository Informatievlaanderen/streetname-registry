namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingMunicipalityOfficialLanguage
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Extensions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityHasAOfficialLanguage : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipalityHasAOfficialLanguage(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
        public void ThenOfficialLanguageWasRemoved(Language language)
        {
            Fixture.Register(() => language);
            var commandLanguageRemoved = Fixture.Create<RemoveOfficialLanguageFromMunicipality>();
            Assert(new Scenario()
                .Given(_streamId, new object[]
                {
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityOfficialLanguageWasAdded>()
                })
                .When(commandLanguageRemoved)
                .Then(new[]
                {
                    new Fact(_streamId, new MunicipalityOfficialLanguageWasRemoved(_municipalityId, language))
                }));
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.English)]
        [InlineData(Language.German)]
        public void AndWasRemoved_ThenNone(Language language)
        {
            Fixture.Register(() => language);
            var commandLanguageRemoved = Fixture.Create<RemoveOfficialLanguageFromMunicipality>();
            Assert(new Scenario()
                .Given(_streamId, new object[]
                {
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityOfficialLanguageWasAdded>(),
                    Fixture.Create<MunicipalityOfficialLanguageWasRemoved>()
                })
                .When(commandLanguageRemoved)
                .ThenNone());
        }

        [Fact]
        public void AndHasMultipleLanguages_TheCorrectOneWasRemoved()
        {
            var commandLanguageRemoved = Fixture.Create<RemoveOfficialLanguageFromMunicipality>().WithLanguage(Language.English);
            var commandAddedEnglish = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.English);
            var commandAddedDutch = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.Dutch);
            Assert(new Scenario()
                .Given(_streamId, new object[]
                {
                    Fixture.Create<MunicipalityWasImported>(),
                    commandAddedEnglish.ToEvent(),
                    commandAddedDutch.ToEvent()
                })
                .When(commandLanguageRemoved)
                .Then(new[]
                {
                    new Fact(_streamId, new MunicipalityOfficialLanguageWasRemoved(_municipalityId, Language.English))
                }));
        }
    }
}
