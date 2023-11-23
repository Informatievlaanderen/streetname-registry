namespace StreetNameRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    /// <summary>
    /// Builder for creating instances of StreetNameHomonymAdditionsWereRemoved.
    /// By default, the languages to remove are dutch.
    /// </summary>
    public class StreetNameHomonymAdditionsWereRemovedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private List<Language>? _languages;
        private PersistentLocalId? _persistentLocalId;

        public StreetNameHomonymAdditionsWereRemovedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameHomonymAdditionsWereRemovedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameHomonymAdditionsWereRemovedBuilder WithLanguages(List<Language> languages)
        {
            _languages = languages;
            return this;
        }

        public StreetNameHomonymAdditionsWereRemovedBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        /// <summary>
        /// Constructs a StreetNameHomonymAdditionsWereRemoved object with optional parameters.
        /// </summary>
        /// <returns>A new instance of StreetNameHomonymAdditionsWereRemoved.</returns>
        public StreetNameHomonymAdditionsWereRemoved Build()
        {
            var streetNameHomonymAdditionsWereRemoved = new StreetNameHomonymAdditionsWereRemoved(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _languages ?? new List<Language> { Language.Dutch });

            ((ISetProvenance)streetNameHomonymAdditionsWereRemoved).SetProvenance(_fixture.Create<Provenance>());
            return streetNameHomonymAdditionsWereRemoved;
        }
    }
}
