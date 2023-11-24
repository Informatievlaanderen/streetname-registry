namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameWasRenamedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private PersistentLocalId? _persistentLocalId;
        private PersistentLocalId? _destinationPersistentLocalId;

        public StreetNameWasRenamedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameWasRenamedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameWasRenamedBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasRenamedBuilder WithDestinationPersistentLocalId(int persistentLocalId)
        {
            _destinationPersistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasRenamed Build()
        {
            var streetNameWasRenamed = new StreetNameWasRenamed(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _destinationPersistentLocalId ?? _fixture.Create<PersistentLocalId>());

            streetNameWasRenamed.SetProvenance(_fixture.Create<Provenance>());

            return streetNameWasRenamed;
        }
    }
}
