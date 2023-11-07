namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameWasProposedV2Builder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private NisCode? _nisCode;
        private PersistentLocalId? _persistentLocalId;
        private Names? _names;

        public StreetNameWasProposedV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameWasProposedV2Builder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameWasProposedV2Builder WithNisCode(string nisCode)
        {
            _nisCode = new NisCode(nisCode);
            return this;
        }

        public StreetNameWasProposedV2Builder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasProposedV2Builder WithNames(Names names)
        {
            _names = names;
            return this;
        }

        public StreetNameWasProposedV2 Build()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _nisCode ?? _fixture.Create<NisCode>(),
                _names ?? _fixture.Create<Names>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>());

            streetNameWasProposedV2.SetProvenance(_fixture.Create<Provenance>());

            return streetNameWasProposedV2;
        }
    }
}
