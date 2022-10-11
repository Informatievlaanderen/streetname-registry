namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;
    using Projections.Legacy.StreetNameNameV2;
    using Xunit;

    public sealed class StreetNameNameProjectionsV2Tests : StreetNameLegacyProjectionTest<StreetNameNameProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public StreetNameNameProjectionsV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenStreetNameWasProposed_ThenStreetNameWasAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            await Sut
                .Given(streetNameWasProposedV2)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
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
                .Given(streetNameWasProposedV2, streetNameWasApproved)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
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
                    streetNameWasProposedV2,
                    streetNameWasApproved,
                    streetNameWasCorrectedFromApprovedToProposed)
                .Then(async ct =>
                {
                    var expectedStreetName = await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
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
                .Given(streetNameWasProposedV2, streetNameWasRejected)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
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
                .Given(streetNameWasProposedV2, streetNameWasApproved, streetNameWasRetiredV2)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
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
                .Given(streetNameWasProposedV2, streetNameNamesWereCorrected)
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameNameV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.English));
                });
        }

        [Fact]
        public async Task WhenMunicipalityNisCodeWasChanged_ThenMunicipalityNisCodeIsUpdatedAndLinkedStreetNamesHaveNewNisCode()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Another = _fixture.Create<StreetNameWasProposedV2>();
            var municipalityNisCodeWasChanged = _fixture.Create<MunicipalityNisCodeWasChanged>();

            await Sut
                .Given(
                    streetNameWasProposedV2,
                    streetNameWasProposedV2Another,
                    municipalityNisCodeWasChanged)
                .Then(async ct =>
                {
                    var expectedStreetNames = ct.StreetNameNamesV2.Where(x =>
                         x.MunicipalityId == municipalityNisCodeWasChanged.MunicipalityId);

                    expectedStreetNames.Select(x => x.NisCode)
                        .Distinct()
                        .Should()
                        .BeEquivalentTo(new List<string> { municipalityNisCodeWasChanged.NisCode });
                    await Task.Yield();
                });
        }

        private string? DetermineExpectedNameForLanguage(IEnumerable<StreetNameName> streetNameNames, Language language)
        {
            return streetNameNames.SingleOrDefault(x => x.Language == language)?.Name;
        }
    }
}
