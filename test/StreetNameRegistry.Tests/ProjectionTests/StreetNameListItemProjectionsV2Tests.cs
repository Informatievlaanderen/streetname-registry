namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;
    using Projections.Legacy.StreetNameListV2;
    using Xunit;

    public sealed class StreetNameListItemProjectionsV2Tests : StreetNameLegacyProjectionTest<StreetNameListProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public StreetNameListItemProjectionsV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenMunicipalityWasImported_ThenMunicipalityWasAdded()
        {
            var municipalityWasImported = _fixture.Create<MunicipalityWasImported>();

            await Sut
                .Given(municipalityWasImported)
                .Then(async ct =>
                {
                    var expectedMunicipality = (await ct.FindAsync<StreetNameListMunicipality>(municipalityWasImported.MunicipalityId));
                    expectedMunicipality.Should().NotBeNull();
                    expectedMunicipality.MunicipalityId.Should().Be(municipalityWasImported.MunicipalityId);
                    expectedMunicipality.NisCode.Should().Be(municipalityWasImported.NisCode);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposed_ThenStreetNameWasAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved_ThenStreetNameStatusWasChangedToApproved()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasCorrectedFromApprovedToProposed = new StreetNameWasCorrectedFromApprovedToProposed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromApprovedToProposed).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasCorrectedFromApprovedToProposed)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected_ThenStreetNameHomonymAdditionsWereCorrected()
        {
            var importMunicipality =
                new MunicipalityWasImported(_fixture.Create<MunicipalityId>(), new NisCode("1011"));
            ((ISetProvenance)importMunicipality).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                _fixture.Create<MunicipalityId>(),
                _fixture.Create<NisCode>(),
                _fixture.Create<StreetNameId>(),
                _fixture.Create<PersistentLocalId>(),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                    new StreetNameName("Rue de la montagne", Language.French),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("XYZ", Language.French),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(_fixture.Create<Provenance>());

            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId),
                new List<StreetNameHomonymAddition>
                {
                    new StreetNameHomonymAddition("DFG", Language.Dutch)
                });
            ((ISetProvenance)streetNameHomonymAdditionsWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    importMunicipality,
                    streetNameWasMigratedToMunicipality,
                    streetNameHomonymAdditionsWereCorrected)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasMigratedToMunicipality.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                    expectedStreetName.HomonymAdditionDutch.Should().Be("DFG");
                    expectedStreetName.HomonymAdditionFrench.Should().Be("XYZ");
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved_ThenStreetNameHomonymAdditionsWereRemoved()
        {
            var importMunicipality =
                new MunicipalityWasImported(_fixture.Create<MunicipalityId>(), new NisCode("1011"));
            ((ISetProvenance)importMunicipality).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                _fixture.Create<MunicipalityId>(),
                _fixture.Create<NisCode>(),
                _fixture.Create<StreetNameId>(),
                _fixture.Create<PersistentLocalId>(),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                    new StreetNameName("Rue de la montagne", Language.French),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("XYZ", Language.French),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(_fixture.Create<Provenance>());

            var streetNameHomonymAdditionsWereRemoved = new StreetNameHomonymAdditionsWereRemoved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId),
                new List<Language> {Language.Dutch} );
            ((ISetProvenance)streetNameHomonymAdditionsWereRemoved).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    importMunicipality,
                    streetNameWasMigratedToMunicipality,
                    streetNameHomonymAdditionsWereRemoved)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasMigratedToMunicipality.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                    expectedStreetName.HomonymAdditionDutch.Should().BeNull();
                    expectedStreetName.HomonymAdditionFrench.Should().Be("XYZ");
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected_ThenStreetNameStatusWasChangedToRejected()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejected = new StreetNameWasRejected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRejected).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasRejected)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenStreetNameStatusWasChangedToRejected()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejectedBecauseOfMunicipalityMerger = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                []);
            ((ISetProvenance)streetNameWasRejectedBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasRejectedBecauseOfMunicipalityMerger)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var streetNameWasRejected = new StreetNameWasRejected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRejected).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasCorrectedFromRejectedToProposed = new StreetNameWasCorrectedFromRejectedToProposed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromRejectedToProposed).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasRejected,
                    streetNameWasCorrectedFromRejectedToProposed)
                .Then(async ct =>
                {
                    var expectedStreetName = await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2_ThenStreetNameStatusWasChangedToRetired()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRetiredV2).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasRetiredV2)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenStreetNameStatusWasChangedToRetired()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRetiredBecauseOfMunicipalityMerger = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                []);
            ((ISetProvenance)streetNameWasRetiredBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasRetiredBecauseOfMunicipalityMerger)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed_ThenStreetNameStatusWasChangedToRetired()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRenamed = new StreetNameWasRenamed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                _fixture.Create<PersistentLocalId>());
            ((ISetProvenance)streetNameWasRenamed).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasRenamed)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent_ThenStreetNameStatusWasChangedBackToCurrent()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRetiredV2).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasCorrectedFromRetiredToCurrent = new StreetNameWasCorrectedFromRetiredToCurrent(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromRetiredToCurrent).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasRetiredV2,
                    streetNameWasRetiredV2,
                    streetNameWasCorrectedFromRetiredToCurrent)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected_ThenStreetNameNamesWereCorrected()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new Names(
                    new[]
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la chapelle", Language.French),
                        new StreetNameName("Kapellenstraate", Language.German),
                        new StreetNameName("Chapel street", Language.English)
                    }));
            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameNamesWereCorrected)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenStreetNameNamesWereChanged()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameNamesWereChanged = new StreetNameNamesWereChanged(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new Names(
                    new[]
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la chapelle", Language.French),
                        new StreetNameName("Kapellenstraate", Language.German),
                        new StreetNameName("Chapel street", Language.English)
                    }));
            ((ISetProvenance)streetNameNamesWereChanged).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameNamesWereChanged)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedV2.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.English));
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
                });
        }

        [Fact]
        public async Task WhenMunicipalityNisCodeWasChanged_ThenMunicipalityNisCodeIsUpdatedAndLinkedStreetNamesHaveNewNisCode()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2_2 = _fixture.Create<StreetNameWasProposedV2>();
            var municipalityWasImported = _fixture.Create<MunicipalityWasImported>();
            var municipalityNisCodeWasChanged = _fixture.Create<MunicipalityNisCodeWasChanged>();

            await Sut
                .Given(
                    municipalityWasImported,
                    streetNameWasProposedV2,
                    streetNameWasProposedV2_2,
                    municipalityNisCodeWasChanged)
                .Then(async ct =>
                {
                    var expectedMunicipality = (await ct.FindAsync<StreetNameListMunicipality>(municipalityNisCodeWasChanged.MunicipalityId));
                    expectedMunicipality.Should().NotBeNull();
                    expectedMunicipality.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);

                    var expectedStreetNames = ct.StreetNameListV2.Where(x =>
                        x.MunicipalityId == municipalityNisCodeWasChanged.MunicipalityId);

                    expectedStreetNames.Select(x => x.NisCode)
                        .Distinct()
                        .Should()
                        .BeEquivalentTo(new List<string> { municipalityNisCodeWasChanged.NisCode });
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2_ThenStreetNameIsRemoved()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRemovedV2 = new StreetNameWasRemovedV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRemovedV2).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasRemovedV2)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.Removed.Should().BeTrue();
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRemovedV2.Provenance.Timestamp);
                });
        }

        private static string? DetermineExpectedNameForLanguage(IDictionary<Language, string> streetNameNames, Language language)
            => streetNameNames.ContainsKey(language) ? streetNameNames[language] : null;
    }
}
