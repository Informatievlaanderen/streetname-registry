namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameNamesWereCorrectedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private PersistentLocalId? _persistentLocalId;
        private Names? _names;

        public StreetNameNamesWereCorrectedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameNamesWereCorrectedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameNamesWereCorrectedBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameNamesWereCorrectedBuilder WithNames(Names names)
        {
            _names = names;
            return this;
        }

        public StreetNameNamesWereCorrected Build()
        {
            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _names ?? _fixture.Create<Names>()
                );

            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            return streetNameNamesWereCorrected;
        }
    }
}
