namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingMunicipalityFacilityLanguage
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
        public void ThenFacilityLanguageWasRemoved(Language language)
        {
            Fixture.Register(() => language);
            var commandLanguageRemoved = Fixture.Create<RemoveFacilityLanguageFromMunicipality>();
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>(), Fixture.Create<MunicipalityFacilityLanguageWasAdded>())
                .When(commandLanguageRemoved)
                .Then(new Fact(_streamId, new MunicipalityFacilityLanguageWasRemoved(_municipalityId, language))));
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.English)]
        [InlineData(Language.German)]
        public void WithAlreadyRemovedLanguage_ThenNone(Language language)
        {
            Fixture.Register(() => language);
            var commandLanguageRemoved = Fixture.Create<RemoveFacilityLanguageFromMunicipality>();
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>(), Fixture.Create<MunicipalityFacilityLanguageWasAdded>(), Fixture.Create<MunicipalityFacilityLanguageWasRemoved>())
                .When(commandLanguageRemoved)
                .ThenNone());
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.English)]
        [InlineData(Language.German)]
        public void WithNonExistingLanguage_ThenNone(Language language)
        {
            Fixture.Register(() => language);
            var commandLanguageRemoved = Fixture.Create<RemoveFacilityLanguageFromMunicipality>();
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>())
                .When(commandLanguageRemoved)
                .ThenNone());
        }

        [Fact]
        public void AndHasMultipleLanguages_TheCorrectOneWasRemoved()
        {
            var commandLanguageRemoved = Fixture.Create<RemoveFacilityLanguageFromMunicipality>().WithLanguage(Language.English);
            var commandAddedEnglish = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.English);
            var commandAddedDutch = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.Dutch);
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>(), commandAddedEnglish.ToEvent(), commandAddedDutch.ToEvent())
                .When(commandLanguageRemoved)
                .Then(new Fact(_streamId, new MunicipalityFacilityLanguageWasRemoved(_municipalityId, Language.English))));
        }
    }
}
