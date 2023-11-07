namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    /// <summary>
    /// Constructs a MunicipalityOfficialLanguageWasAdded object with optional parameters.
    /// By default, the Language is Dutch
    /// </summary>
    /// <returns>MunicipalityOfficialLanguageWasAdded</returns>
    public class MunicipalityOfficialLanguageWasAddedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private Language? _language;

        public MunicipalityOfficialLanguageWasAddedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public MunicipalityOfficialLanguageWasAddedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public MunicipalityOfficialLanguageWasAddedBuilder WithLanguage(Language language)
        {
            _language = language;
            return this;
        }

        /// <summary>
        /// Constructs a MunicipalityOfficialLanguageWasAdded object with optional parameters.
        /// </summary>
        /// <returns>A new instance of MunicipalityOfficialLanguageWasAdded.</returns>
        public MunicipalityOfficialLanguageWasAdded Build()
        {
            var municipalityOfficialLanguageWasAdded =
                new MunicipalityOfficialLanguageWasAdded(_municipalityId ?? _fixture.Create<MunicipalityId>(), _language ?? Language.Dutch);
            municipalityOfficialLanguageWasAdded.SetProvenance(_fixture.Create<Provenance>());
            return municipalityOfficialLanguageWasAdded;
        }
    }
}
