namespace StreetNameRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;

    /// <summary>
    /// Builder for creating instances of StreetNameHomonymAdditionsWereRemoved.
    /// By default, the homonym additions to remove is an empty list.
    /// </summary>
    public class CorrectStreetNameHomonymAdditionsBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private PersistentLocalId? _persistentLocalId;
        private HomonymAdditions? _homonymAdditions;
        private List<Language>? _homonymAdditionsToRemove;

        public CorrectStreetNameHomonymAdditionsBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public CorrectStreetNameHomonymAdditionsBuilder WithMunicipalityId(MunicipalityId? municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public CorrectStreetNameHomonymAdditionsBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public CorrectStreetNameHomonymAdditionsBuilder WithHomonymAdditions(HomonymAdditions? homonymAdditions)
        {
            _homonymAdditions = homonymAdditions;
            return this;
        }

        public CorrectStreetNameHomonymAdditionsBuilder WithHomonymAdditionsToRemove(List<Language>? homonymAdditionsToRemove)
        {
            _homonymAdditionsToRemove = homonymAdditionsToRemove;
            return this;
        }

        /// <summary>
        /// Constructs a SCorrectStreetNameHomonymAdditions object with optional parameters.
        /// </summary>
        /// <returns>A new instance of CorrectStreetNameHomonymAdditions.</returns>
        public CorrectStreetNameHomonymAdditions Build()
        {
            return new CorrectStreetNameHomonymAdditions(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _homonymAdditions ?? _fixture.Create<HomonymAdditions>(),
                _homonymAdditionsToRemove ?? new List<Language>(),
                _fixture.Create<Provenance>());
        }
    }
}
