namespace StreetNameRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameHomonymAdditionsWereCorrectedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private PersistentLocalId? _persistentLocalId;
        private List<StreetNameHomonymAddition>? _homonymAdditions;

        public StreetNameHomonymAdditionsWereCorrectedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameHomonymAdditionsWereCorrectedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameHomonymAdditionsWereCorrectedBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameHomonymAdditionsWereCorrectedBuilder WithHomonymAdditions(List<StreetNameHomonymAddition> additions)
        {
            _homonymAdditions = additions;
            return this;
        }

        public StreetNameHomonymAdditionsWereCorrected Build()
        {
            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrected(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _homonymAdditions ?? new List<StreetNameHomonymAddition>());

            ((ISetProvenance)streetNameHomonymAdditionsWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            return streetNameHomonymAdditionsWereCorrected;
        }
    }
}
