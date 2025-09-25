namespace StreetNameRegistry.Tests.ProjectionTests.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Builders;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Municipality.Events;
    using NetTopologySuite.Geometries;
    using Projections.Elastic;
    using Projections.Elastic.StreetNameList;
    using Projections.Integration;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Tests.BackOffice;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;
//TODO-pr unit tests
    public class StreetNameListProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly ConnectedProjectionTest<ElasticRunnerContext, StreetNameListProjections> _sut;

        private readonly Mock<IStreetNameListElasticClient> _elasticSearchClient;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly Mock<IDbContextFactory<SyndicationContext>> _syndicationContextFactory;

        public StreetNameListProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());

            _elasticSearchClient = new Mock<IStreetNameListElasticClient>();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _syndicationContextFactory = new Mock<IDbContextFactory<SyndicationContext>>();
            _syndicationContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_syndicationContext);

            _sut = new ConnectedProjectionTest<ElasticRunnerContext, StreetNameListProjections>(CreateContext, CreateProjection);
        }

        private ElasticRunnerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ElasticRunnerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElasticRunnerContext(options);
        }

        private StreetNameListProjections CreateProjection() => new(
            _elasticSearchClient.Object,
            _syndicationContextFactory.Object);

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
        public async Task WhenStreetNameWasProposedForMunicipalityMerger_ThenStreetNameWasAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMergerBuilder(_fixture)
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("XYZ", Language.French),
                }))
                .Build();

            await Sut
                .Given(
                    _fixture.Create<MunicipalityWasImported>(),
                    streetNameWasProposedForMunicipalityMerger)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameListItemV2>(streetNameWasProposedForMunicipalityMerger.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedForMunicipalityMerger.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedForMunicipalityMerger.NisCode);
                    expectedStreetName.IsInFlemishRegion.Should().Be(RegionFilter.IsFlemishRegion(streetNameWasProposedForMunicipalityMerger.NisCode));
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedForMunicipalityMerger.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedForMunicipalityMerger.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedForMunicipalityMerger.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedForMunicipalityMerger.StreetNameNames, Language.English));
                    expectedStreetName.HomonymAdditionDutch.Should().Be("ABC");
                    expectedStreetName.HomonymAdditionFrench.Should().Be("XYZ");
                    expectedStreetName.PrimaryLanguage.Should().Be(null);
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

        // [Fact]
        // public async Task WhenStreetNameWasMigratedToStreetName()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameWasMigratedToStreetName>()
        //         .WithPosition(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     var municipalityLatestItem = new MunicipalityLatestItem
        //     {
        //         NisCode = streetNameLatestItem.NisCode,
        //         NameDutch = "Gent",
        //         NameFrench = "Gand"
        //     };
        //     _syndicationContext.MunicipalityLatestItems.Add(municipalityLatestItem);
        //     await _syndicationContext.SaveChangesAsync();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode!,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasMigratedToStreetName>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.CreateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.ParentStreetNamePersistentLocalId == @event.ParentPersistentLocalId
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.Status == @event.Status
        //                     && doc.OfficiallyAssigned == @event.OfficiallyAssigned
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.Municipality.NisCode == municipalityLatestItem.NisCode
        //                     && doc.Municipality.Names.Length == 2
        //                     && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.StreetName.Names.Length == 2
        //                     && doc.StreetName.HomonymAdditions.Length == 2
        //                     && doc.StreetNamePosition.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification
        //                     && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasProposedV2()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameWasProposedV2>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     var municipalityLatestItem = new MunicipalityLatestItem
        //     {
        //         NisCode = streetNameLatestItem.NisCode,
        //         NameDutch = "Gent",
        //         NameFrench = "Gand"
        //     };
        //     _syndicationContext.MunicipalityLatestItems.Add(municipalityLatestItem);
        //     await _syndicationContext.SaveChangesAsync();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasProposedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.CreateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.ParentStreetNamePersistentLocalId == @event.ParentPersistentLocalId
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.Status == StreetNameStatus.Proposed
        //                     && doc.OfficiallyAssigned == true
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.Municipality.NisCode == municipalityLatestItem.NisCode
        //                     && doc.Municipality.Names.Length == 2
        //                     && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.StreetName.Names.Length == 2
        //                     && doc.StreetName.HomonymAdditions.Length == 2
        //                     && doc.StreetNamePosition.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification &&
        //                     doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasProposedBecauseOfRestreetName()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameWasProposedBecauseOfRestreetName>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     var municipalityLatestItem = new MunicipalityLatestItem
        //     {
        //         NisCode = streetNameLatestItem.NisCode,
        //         NameDutch = "Gent",
        //         NameFrench = "Gand"
        //     };
        //     _syndicationContext.MunicipalityLatestItems.Add(municipalityLatestItem);
        //     await _syndicationContext.SaveChangesAsync();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasProposedBecauseOfRestreetName>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.CreateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.ParentStreetNamePersistentLocalId == @event.ParentPersistentLocalId
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.Status == StreetNameStatus.Proposed
        //                     && doc.OfficiallyAssigned == true
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.Municipality.NisCode == municipalityLatestItem.NisCode
        //                     && doc.Municipality.Names.Length == 2
        //                     && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.StreetName.Names.Length == 2
        //                     && doc.StreetName.HomonymAdditions.Length == 2
        //                     && doc.StreetNamePosition.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification &&
        //                     doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasProposedForMunicipalityMerger()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameWasProposedForMunicipalityMerger>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     var municipalityLatestItem = new MunicipalityLatestItem
        //     {
        //         NisCode = streetNameLatestItem.NisCode,
        //         NameDutch = "Gent",
        //         NameFrench = "Gand"
        //     };
        //     _syndicationContext.MunicipalityLatestItems.Add(municipalityLatestItem);
        //     await _syndicationContext.SaveChangesAsync();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasProposedForMunicipalityMerger>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.CreateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.ParentStreetNamePersistentLocalId == @event.ParentPersistentLocalId
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.Status == StreetNameStatus.Proposed
        //                     && doc.OfficiallyAssigned == @event.OfficiallyAssigned
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.Municipality.NisCode == municipalityLatestItem.NisCode
        //                     && doc.Municipality.Names.Length == 2
        //                     && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.StreetName.Names.Length == 2
        //                     && doc.StreetName.HomonymAdditions.Length == 2
        //                     && doc.StreetNamePosition.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification &&
        //                     doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasApproved()
        // {
        //     var @event = _fixture.Create<StreetNameWasApproved>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasApproved>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Current
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasCorrectedFromApprovedToProposed()
        // {
        //     var @event = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposed>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasCorrectedFromApprovedToProposed>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Proposed
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Proposed
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasCorrectedFromRejectedToProposed()
        // {
        //     var @event = _fixture.Create<StreetNameWasCorrectedFromRejectedToProposed>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasCorrectedFromRejectedToProposed>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Proposed
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejected()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseHouseNumberWasRejected()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseHouseNumberWasRejected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseHouseNumberWasRejected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseHouseNumberWasRetired()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseHouseNumberWasRetired>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseStreetNameWasRejected()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseHouseNumberWasRetired>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseStreetNameWasRetired()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseStreetNameWasRetired>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseStreetNameWasRetired>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseOfRestreetName()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseOfRestreetName>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseOfRestreetName>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger()
        // {
        //     var @event = _fixture.Create<StreetNameWasRejectedBecauseOfMunicipalityMerger>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Rejected
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredV2()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseStreetNameWasRejected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseStreetNameWasRejected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredBecauseHouseNumberWasRetired()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseHouseNumberWasRetired>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredBecauseStreetNameWasRejected()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseStreetNameWasRejected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseStreetNameWasRejected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredBecauseStreetNameWasRetired()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseStreetNameWasRetired>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseStreetNameWasRetired>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredBecauseOfRestreetName()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseOfRestreetName>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseOfRestreetName>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger()
        // {
        //     var @event = _fixture.Create<StreetNameWasRetiredBecauseOfMunicipalityMerger>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Retired
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent()
        // {
        //     var @event = _fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasCorrectedFromRetiredToCurrent>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Current
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                     && doc.OfficiallyAssigned == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasDeregulated()
        // {
        //     var @event = _fixture.Create<StreetNameWasDeregulated>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasDeregulated>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.Status == StreetNameStatus.Current
        //                     && doc.OfficiallyAssigned == false
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameDeregulationWasCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameDeregulationWasCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameDeregulationWasCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.OfficiallyAssigned == true
        //                     && doc.Status == null
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRegularized()
        // {
        //     var @event = _fixture.Create<StreetNameWasRegularized>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRegularized>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.OfficiallyAssigned == true
        //                     && doc.Status == null
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameRegularizationWasCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameRegularizationWasCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameRegularizationWasCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.OfficiallyAssigned == false
        //                     && doc.Status == StreetNameStatus.Current
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.StreetNamePosition == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNamePostalCodeWasChangedV2()
        // {
        //     var @event = _fixture.Create<StreetNamePostalCodeWasChangedV2>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = new[] { @event.StreetNamePersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNamePostalCodeWasChangedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.PostalInfo!.PostalCode == @event.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.PostalInfo.Names.Single().Language == Language.nl
        //                     && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             foreach (var boxNumberStreetNamePersistentLocalId in @event.BoxNumberPersistentLocalIds)
        //             {
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == boxNumberStreetNamePersistentLocalId
        //                         && doc.PostalInfo!.PostalCode == @event.PostalCode
        //                         && doc.PostalInfo.Names.Length == 1
        //                         && doc.PostalInfo.Names.Single().Language == Language.nl
        //                         && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
        //                         && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNamePostalCodeWasCorrectedV2()
        // {
        //     var @event = _fixture.Create<StreetNamePostalCodeWasCorrectedV2>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = new[] { @event.StreetNamePersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNamePostalCodeWasCorrectedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.PostalInfo!.PostalCode == @event.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.PostalInfo.Names.Single().Language == Language.nl
        //                     && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             foreach (var boxNumberStreetNamePersistentLocalId in @event.BoxNumberPersistentLocalIds)
        //             {
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == boxNumberStreetNamePersistentLocalId
        //                         && doc.PostalInfo!.PostalCode == @event.PostalCode
        //                         && doc.PostalInfo.Names.Length == 1
        //                         && doc.PostalInfo.Names.Single().Language == Language.nl
        //                         && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
        //                         && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameHouseNumberWasCorrectedV2()
        // {
        //     var @event = _fixture.Create<StreetNameHouseNumberWasCorrectedV2>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = new[] { @event.StreetNamePersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameHouseNumberWasCorrectedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             foreach (var boxNumberStreetNamePersistentLocalId in @event.BoxNumberPersistentLocalIds)
        //             {
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == boxNumberStreetNamePersistentLocalId
        //                         && doc.HouseNumber == @event.HouseNumber
        //                         && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameBoxNumberWasCorrectedV2()
        // {
        //     var @event = _fixture.Create<StreetNameBoxNumberWasCorrectedV2>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = new[] { @event.StreetNamePersistentLocalId }
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameBoxNumberWasCorrectedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameBoxNumbersWereCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameBoxNumbersWereCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameBoxNumbersWereCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             foreach (var (streetNamePersistentLocalId, boxNumber) in @event.StreetNameBoxNumbers)
        //             {
        //                 _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                     streetNamePersistentLocalId,
        //                     It.Is<StreetNameListPartialDocument>(doc =>
        //                         doc.BoxNumber == boxNumber
        //                         && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                         && doc.Status == null
        //                         && doc.StreetNamePosition == null
        //                         && doc.OfficiallyAssigned == null
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNamePositionWasChanged()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNamePositionWasChanged>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNamePositionWasChanged>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.StreetNamePosition!.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification
        //                     && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.OfficiallyAssigned == null
        //                     && doc.Status == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNamePositionWasCorrectedV2()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNamePositionWasCorrectedV2>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNamePositionWasCorrectedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.Is<StreetNameListPartialDocument>(doc =>
        //                     doc.StreetNamePosition!.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification
        //                     && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.OfficiallyAssigned == null
        //                     && doc.Status == null
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameHouseNumberWasRestreetNameed()
        // {
        //     _fixture.Customize(new WithFixedPostalCode());
        //
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameHouseNumberWasRestreetNameed>()
        //         .WithExtendedWkbGeometry(new ExtendedWkbGeometry(expectedPosition));
        //
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = new[] { @event.StreetNamePersistentLocalId }.Concat(
        //             @event.RestreetNameedBoxNumbers.Select(x => x.DestinationStreetNamePersistentLocalId))
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             return document;
        //         })
        //         .ToArray();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     var postalLatestItem = new PostalLatestItem
        //     {
        //         PostalCode = _fixture.Create<PostalCode>(),
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     };
        //     _postalConsumerContext.PostalLatestItems.Add(postalLatestItem);
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameHouseNumberWasRestreetNameed>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.HouseNumber == @event.RestreetNameedHouseNumber.DestinationHouseNumber
        //                     && doc.PostalInfo!.PostalCode == postalLatestItem.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.Status == @event.RestreetNameedHouseNumber.SourceStatus
        //                     && doc.OfficiallyAssigned == @event.RestreetNameedHouseNumber.SourceIsOfficiallyAssigned
        //                     && doc.StreetNamePosition.GeometryMethod == @event.RestreetNameedHouseNumber.SourceGeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.RestreetNameedHouseNumber.SourceGeometrySpecification
        //                     && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             foreach (var boxNumberStreetName in @event.RestreetNameedBoxNumbers)
        //             {
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == boxNumberStreetName.DestinationStreetNamePersistentLocalId
        //                         && doc.HouseNumber == boxNumberStreetName.DestinationHouseNumber
        //                         && doc.BoxNumber == boxNumberStreetName.SourceBoxNumber
        //                         && doc.PostalInfo!.PostalCode == postalLatestItem.PostalCode
        //                         && doc.PostalInfo.Names.Length == 1
        //                         && doc.Status == boxNumberStreetName.SourceStatus
        //                         && doc.OfficiallyAssigned == boxNumberStreetName.SourceIsOfficiallyAssigned
        //                         && doc.StreetNamePosition.GeometryMethod == boxNumberStreetName.SourceGeometryMethod
        //                         && doc.StreetNamePosition.GeometrySpecification == boxNumberStreetName.SourceGeometrySpecification
        //                         && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                         && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                         && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRemovedV2()
        // {
        //     var @event = _fixture.Create<StreetNameWasRemovedV2>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRemovedV2>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.DeleteDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRemovedBecauseHouseNumberWasRemoved()
        // {
        //     var @event = _fixture.Create<StreetNameWasRemovedBecauseHouseNumberWasRemoved>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.DeleteDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameWasRemovedBecauseStreetNameWasRemoved()
        // {
        //     var @event = _fixture.Create<StreetNameWasRemovedBecauseStreetNameWasRemoved>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameWasRemovedBecauseStreetNameWasRemoved>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.DeleteDocument(
        //                 @event.StreetNamePersistentLocalId,
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameRemovalWasCorrected()
        // {
        //     var expectedPosition = GeometryHelpers.ExampleExtendedWkb;
        //     var expectedPoint = (Point)WKBReaderFactory.Create().Read(expectedPosition);
        //     var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(expectedPoint);
        //     var @event = _fixture.Create<StreetNameRemovalWasCorrected>()
        //         .WithGeometry(new ExtendedWkbGeometry(expectedPosition));
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     var municipalityLatestItem = new MunicipalityLatestItem
        //     {
        //         NisCode = streetNameLatestItem.NisCode,
        //         NameDutch = "Gent",
        //         NameFrench = "Gand"
        //     };
        //     _syndicationContext.MunicipalityLatestItems.Add(municipalityLatestItem);
        //     await _syndicationContext.SaveChangesAsync();
        //
        //     var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
        //     _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
        //     {
        //         PostalCode = @event.PostalCode!,
        //         PostalNames = new List<PostalInfoPostalName>
        //         {
        //             postalInfoPostalName
        //         }
        //     });
        //     await _postalConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameRemovalWasCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             _elasticSearchClient.Verify(x => x.CreateDocument(
        //                 It.Is<StreetNameListDocument>(doc =>
        //                     doc.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.ParentStreetNamePersistentLocalId == @event.ParentPersistentLocalId
        //                     && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
        //                     && doc.Status == @event.Status
        //                     && doc.OfficiallyAssigned == @event.OfficiallyAssigned
        //                     && doc.HouseNumber == @event.HouseNumber
        //                     && doc.BoxNumber == @event.BoxNumber
        //                     && doc.Municipality.NisCode == municipalityLatestItem.NisCode
        //                     && doc.Municipality.Names.Length == 2
        //                     && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
        //                     && doc.PostalInfo.Names.Length == 1
        //                     && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                     && doc.StreetName.Names.Length == 2
        //                     && doc.StreetName.HomonymAdditions.Length == 2
        //                     && doc.StreetNamePosition.GeometryMethod == @event.GeometryMethod
        //                     && doc.StreetNamePosition.GeometrySpecification == @event.GeometrySpecification
        //                     && doc.StreetNamePosition.GeometryAsWkt == expectedPoint.AsText()
        //                     && doc.StreetNamePosition.GeometryAsWgs84 == pointAsWgs84
        //                 ),
        //                 It.IsAny<CancellationToken>()));
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameNamesWereChanged()
        // {
        //     var @event = _fixture.Create<StreetNameNamesWereChanged>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = @event.StreetNamePersistentLocalIds
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             document.VersionTimestamp = _fixture.Create<DateTimeOffset>();
        //             document.PostalInfo = new PostalInfo(
        //                 _fixture.Create<string>(),
        //                 [
        //                     new Name(_fixture.Create<string>(), Language.nl),
        //                     new Name(_fixture.Create<string>(), Language.fr),
        //                     new Name(_fixture.Create<string>(), Language.en)
        //                 ]);
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameNamesWereChanged>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             foreach (var streetNamePersistentLocalId in @event.StreetNamePersistentLocalIds)
        //             {
        //                 var expectedVersionTimeStamp = @event.Provenance.Timestamp.ToBelgianDateTimeOffset();
        //                 var storedDocument = storedDocuments.Single(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
        //                 if (expectedVersionTimeStamp < storedDocument.VersionTimestamp)
        //                 {
        //                     expectedVersionTimeStamp = storedDocument.VersionTimestamp;
        //                 }
        //
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == streetNamePersistentLocalId
        //                         && doc.VersionTimestamp == expectedVersionTimeStamp
        //                         && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                         && doc.StreetName.Names.Length == 2
        //                         && doc.StreetName.HomonymAdditions.Length == 2
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameNamesWereCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameNamesWereCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = @event.StreetNamePersistentLocalIds
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             document.VersionTimestamp = _fixture.Create<DateTimeOffset>();
        //             document.PostalInfo = new PostalInfo(
        //                 _fixture.Create<string>(),
        //                 [
        //                     new Name(_fixture.Create<string>(), Language.nl),
        //                     new Name(_fixture.Create<string>(), Language.fr),
        //                     new Name(_fixture.Create<string>(), Language.en)
        //                 ]);
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameNamesWereCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             foreach (var streetNamePersistentLocalId in @event.StreetNamePersistentLocalIds)
        //             {
        //                 var expectedVersionTimeStamp = @event.Provenance.Timestamp.ToBelgianDateTimeOffset();
        //                 var storedDocument = storedDocuments.Single(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
        //                 if (expectedVersionTimeStamp < storedDocument.VersionTimestamp)
        //                 {
        //                     expectedVersionTimeStamp = storedDocument.VersionTimestamp;
        //                 }
        //
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == streetNamePersistentLocalId
        //                         && doc.VersionTimestamp == expectedVersionTimeStamp
        //                         && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                         && doc.StreetName.Names.Length == 2
        //                         && doc.StreetName.HomonymAdditions.Length == 2
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameHomonymAdditionsWereCorrected()
        // {
        //     var @event = _fixture.Create<StreetNameHomonymAdditionsWereCorrected>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = @event.StreetNamePersistentLocalIds
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             document.VersionTimestamp = _fixture.Create<DateTimeOffset>();
        //             document.PostalInfo = new PostalInfo(
        //                 _fixture.Create<string>(),
        //                 [
        //                     new Name(_fixture.Create<string>(), Language.nl),
        //                     new Name(_fixture.Create<string>(), Language.fr),
        //                     new Name(_fixture.Create<string>(), Language.en)
        //                 ]);
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             foreach (var streetNamePersistentLocalId in @event.StreetNamePersistentLocalIds)
        //             {
        //                 var expectedVersionTimeStamp = @event.Provenance.Timestamp.ToBelgianDateTimeOffset();
        //                 var storedDocument = storedDocuments.Single(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
        //                 if (expectedVersionTimeStamp < storedDocument.VersionTimestamp)
        //                 {
        //                     expectedVersionTimeStamp = storedDocument.VersionTimestamp;
        //                 }
        //
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == streetNamePersistentLocalId
        //                         && doc.VersionTimestamp == expectedVersionTimeStamp
        //                         && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                         && doc.StreetName.Names.Length == 2
        //                         && doc.StreetName.HomonymAdditions.Length == 2
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenStreetNameHomonymAdditionsWereRemoved()
        // {
        //     var @event = _fixture.Create<StreetNameHomonymAdditionsWereRemoved>();
        //     var eventMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
        //         { Envelope.PositionMetadataKey, _fixture.Create<long>() }
        //     };
        //
        //     var storedDocuments = @event.StreetNamePersistentLocalIds
        //         .Select(x =>
        //         {
        //             var document = _fixture.Create<StreetNameListDocument>();
        //             document.StreetNamePersistentLocalId = x;
        //             document.VersionTimestamp = _fixture.Create<DateTimeOffset>();
        //             document.PostalInfo = new PostalInfo(
        //                 _fixture.Create<string>(),
        //                 [
        //                     new Name(_fixture.Create<string>(), Language.nl),
        //                     new Name(_fixture.Create<string>(), Language.fr),
        //                     new Name(_fixture.Create<string>(), Language.en)
        //                 ]);
        //             return document;
        //         })
        //         .ToArray();
        //
        //     _elasticSearchClient
        //         .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(storedDocuments);
        //
        //     var streetNameLatestItem = new StreetNameLatestItem
        //     {
        //         PersistentLocalId = @event.StreetNamePersistentLocalId,
        //         NisCode = "44021",
        //         NameDutch = "Bosstraat",
        //         NameFrench = "Rue Forestière",
        //         HomonymAdditionDutch = "MA",
        //         HomonymAdditionFrench = "AM"
        //     };
        //     _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
        //     await _streetNameConsumerContext.SaveChangesAsync();
        //
        //     await _sut
        //         .Given(new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(@event, eventMetadata)))
        //         .Then(_ =>
        //         {
        //             foreach (var streetNamePersistentLocalId in @event.StreetNamePersistentLocalIds)
        //             {
        //                 var expectedVersionTimeStamp = @event.Provenance.Timestamp.ToBelgianDateTimeOffset();
        //                 var storedDocument = storedDocuments.Single(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
        //                 if (expectedVersionTimeStamp < storedDocument.VersionTimestamp)
        //                 {
        //                     expectedVersionTimeStamp = storedDocument.VersionTimestamp;
        //                 }
        //
        //                 _elasticSearchClient.Verify(x => x.UpdateDocument(
        //                     It.Is<StreetNameListDocument>(doc =>
        //                         doc.StreetNamePersistentLocalId == streetNamePersistentLocalId
        //                         && doc.VersionTimestamp == expectedVersionTimeStamp
        //                         && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
        //                         && doc.StreetName.Names.Length == 2
        //                         && doc.StreetName.HomonymAdditions.Length == 2
        //                     ),
        //                     It.IsAny<CancellationToken>()));
        //             }
        //
        //             return Task.CompletedTask;
        //         });
        // }
    }
}
