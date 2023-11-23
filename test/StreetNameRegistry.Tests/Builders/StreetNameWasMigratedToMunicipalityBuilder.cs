namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    /// <summary>
    /// Builder for creating instances of StreetNameWasMigratedToMunicipality.
    /// By default, the primary language is Dutch, IsCompleted is true, and IsRemoved is false.
    /// </summary>
    public class StreetNameWasMigratedToMunicipalityBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private NisCode? _nisCode;
        private StreetNameId? _streetNameId;
        private PersistentLocalId? _persistentLocalId;
        private StreetNameStatus? _status;
        private Language? _primaryLanguage;
        private Language? _secondaryLanguage;
        private Names? _names;
        private HomonymAdditions? _homonymAdditions;
        private bool _isCompleted = true;
        private bool _isRemoved;

        public StreetNameWasMigratedToMunicipalityBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithNisCode(NisCode nisCode)
        {
            _nisCode = nisCode;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithStreetNameId(StreetNameId streetNameId)
        {
            _streetNameId = streetNameId;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithStatus(StreetNameStatus streetNameStatus)
        {
            _status = streetNameStatus;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithPrimaryLanguage(Language language)
        {
            _primaryLanguage = language;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithSecondaryLanguage(Language language)
        {
            _secondaryLanguage = language;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithNames(Names names)
        {
            _names = names;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithHomonymAdditions(HomonymAdditions additions)
        {
            _homonymAdditions = additions;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithUnCompleted()
        {
            _isCompleted = false;
            return this;
        }

        public StreetNameWasMigratedToMunicipalityBuilder WithIsRemoved()
        {
            _isRemoved = true;
            return this;
        }

        /// <summary>
        /// Constructs a StreetNameWasMigratedToMunicipality object with optional parameters.
        /// </summary>
        /// <returns>A new instance of StreetNameWasMigratedToMunicipality.</returns>
        public StreetNameWasMigratedToMunicipality Build()
        {
            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _nisCode ?? _fixture.Create<NisCode>(),
                _streetNameId ?? _fixture.Create<StreetNameId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _status ?? _fixture.Create<StreetNameStatus>(),
                _primaryLanguage ?? Language.Dutch,
                _secondaryLanguage ?? _fixture.Create<Language>(),
                _names ?? _fixture.Create<Names>(),
                _homonymAdditions ?? _fixture.Create<HomonymAdditions>(),
                _isCompleted,
                _isRemoved);

            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(_fixture.Create<Provenance>());

            return streetNameWasMigratedToMunicipality;
        }
    }
}
