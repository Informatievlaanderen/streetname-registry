namespace StreetNameRegistry.Tests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameWasApprovedBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private PersistentLocalId? _persistentLocalId;

        public StreetNameWasApprovedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameWasApprovedBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameWasApprovedBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasApproved Build()
        {
            var streetNameWasApproved = new StreetNameWasApproved(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>());

            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());

            return streetNameWasApproved;
        }
    }
}
