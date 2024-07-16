namespace StreetNameRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;

    public class StreetNameWasProposedForMunicipalityMergerBuilder
    {
        private readonly Fixture _fixture;
        private MunicipalityId? _municipalityId;
        private NisCode? _nisCode;
        private PersistentLocalId? _persistentLocalId;
        private Names? _names;
        private HomonymAdditions? _homonymAdditions;
        private List<PersistentLocalId>? _mergedStreetNamePersistentLocalIds;

        public StreetNameWasProposedForMunicipalityMergerBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithMunicipalityId(MunicipalityId municipalityId)
        {
            _municipalityId = municipalityId;
            return this;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithNisCode(string nisCode)
        {
            _nisCode = new NisCode(nisCode);
            return this;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithPersistentLocalId(int persistentLocalId)
        {
            _persistentLocalId = new PersistentLocalId(persistentLocalId);
            return this;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithNames(Names names)
        {
            _names = names;
            return this;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithHomonymAdditions(HomonymAdditions homonymAdditions)
        {
            _homonymAdditions = homonymAdditions;
            return this;
        }

        public StreetNameWasProposedForMunicipalityMergerBuilder WithMergedStreetNamePersistentLocalIds(List<PersistentLocalId>? mergedStreetNamePersistentLocalIds)
        {
            _mergedStreetNamePersistentLocalIds = mergedStreetNamePersistentLocalIds;
            return this;
        }

        public StreetNameWasProposedForMunicipalityMerger Build()
        {
            var StreetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMerger(
                _municipalityId ?? _fixture.Create<MunicipalityId>(),
                _nisCode ?? _fixture.Create<NisCode>(),
                _names ?? _fixture.Create<Names>(),
                _homonymAdditions ?? _fixture.Create<HomonymAdditions>(),
                _persistentLocalId ?? _fixture.Create<PersistentLocalId>(),
                _mergedStreetNamePersistentLocalIds ?? _fixture.CreateMany<int>(5).Select(x => new PersistentLocalId(x)).ToList());

            StreetNameWasProposedForMunicipalityMerger.SetProvenance(_fixture.Create<Provenance>());

            return StreetNameWasProposedForMunicipalityMerger;
        }
    }
}
