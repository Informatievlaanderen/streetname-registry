namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;
    using Projections.Legacy.StreetNameDetailV2;
    using Xunit;

    public sealed class StreetNameLatestItemProjectionsV2Tests : StreetNameLegacyProjectionTest<StreetNameDetailProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public StreetNameLatestItemProjectionsV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task WhenStreetNameWasProposedV2_ThenNewStreetNameWasAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasProposedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved_ThenStreetNameStatusWasChangedToApproved()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasApproved.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            var streetNameWasCorrectedFromApprovedToProposed = new StreetNameWasCorrectedFromApprovedToProposed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromApprovedToProposed).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasCorrectedFromApprovedToProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasCorrectedFromApprovedToProposed.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)),
                    new Envelope<StreetNameWasCorrectedFromApprovedToProposed>(new Envelope(streetNameWasCorrectedFromApprovedToProposed, streetNameWasCorrectedFromApprovedToProposedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasCorrectedFromApprovedToProposed.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected_ThenStreetNameStatusWasChangedToRejected()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasRejected = new StreetNameWasRejected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRejected).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRejected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasRejected>(new Envelope(streetNameWasRejected, streetNameWasRejectedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasRejected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasRejected = new StreetNameWasRejected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRejected).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRejectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRejected.GetHash() }
            };

            var streetNameWasCorrectedFromRejectedToProposed = new StreetNameWasCorrectedFromRejectedToProposed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromRejectedToProposed).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasCorrectedFromRejectedToProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasCorrectedFromRejectedToProposed.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasRejected>(new Envelope(streetNameWasRejected, streetNameWasRejectedMetadata)),
                    new Envelope<StreetNameWasCorrectedFromRejectedToProposed>(new Envelope(streetNameWasCorrectedFromRejectedToProposed, streetNameWasCorrectedFromRejectedToProposedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasCorrectedFromRejectedToProposed.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2_ThenStreetNameStatusWasChangedToRetired()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRetiredV2).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRetiredV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRetiredV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)),
                    new Envelope<StreetNameWasRetiredV2>(new Envelope(streetNameWasRetiredV2, streetNameWasRetiredV2Metadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasRetiredV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed_ThenStreetNameStatusWasChangedToRetired()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            var streetNameWasRenamed = new StreetNameWasRenamed(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                _fixture.Create<PersistentLocalId>());
            ((ISetProvenance)streetNameWasRenamed).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRenamedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRenamed.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)),
                    new Envelope<StreetNameWasRenamed>(new Envelope(streetNameWasRenamed, streetNameWasRenamedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasRenamed.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent_ThenStreetNameStatusWasChangedBackToCurrent()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRetiredV2).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRetiredV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRetiredV2.GetHash() }
            };

            var streetNameWasCorrectedFromRetiredToCurrent = new StreetNameWasCorrectedFromRetiredToCurrent(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasCorrectedFromRetiredToCurrent).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasCorrectedFromRetiredToCurrentToProposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasCorrectedFromRetiredToCurrent.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)),
                    new Envelope<StreetNameWasRetiredV2>(new Envelope(streetNameWasRetiredV2, streetNameWasRetiredV2Metadata)),
                    new Envelope<StreetNameWasCorrectedFromRetiredToCurrent>(new Envelope(streetNameWasCorrectedFromRetiredToCurrent, streetNameWasCorrectedFromRetiredToCurrentToProposedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected_ThenStreetNameNamesWereCorrected()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new Names(
                    new []
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la chapelle", Language.French),
                        new StreetNameName("Kapellenstraate", Language.German),
                        new StreetNameName("Chapel street", Language.English)
                    }));

            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(_fixture.Create<Provenance>());
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, metadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereCorrected.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenStreetNameNamesWereChanged()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameNamesWereChanged = new StreetNameNamesWereChanged(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new Names(
                    new []
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la chapelle", Language.French),
                        new StreetNameName("Kapellenstraate", Language.German),
                        new StreetNameName("Chapel street", Language.English)
                    }));

            ((ISetProvenance)streetNameNamesWereChanged).SetProvenance(_fixture.Create<Provenance>());
            var streetNameNamesWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameNamesWereChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, metadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, streetNameNamesWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeFalse();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameNamesWereChanged.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameNamesWereChanged.GetHash());
                });
        }

        private static string? DetermineExpectedNameForLanguage(IDictionary<Language, string> streetNameNames, Language language)
            => streetNameNames.ContainsKey(language) ? streetNameNames[language] : null;

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected_ThenStreetNameHomonymAdditionsWereCorrected()
        {
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

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasMigratedToMunicipality.GetHash() }
            };

            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId),
                new List<StreetNameHomonymAddition>
                {
                    new StreetNameHomonymAddition("DFG", Language.Dutch)
                });
            ((ISetProvenance)streetNameHomonymAdditionsWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            var streetNameHomonymAdditionsWereCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereCorrected.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, metadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, streetNameHomonymAdditionsWereCorrectedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasMigratedToMunicipality.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                    expectedStreetName.HomonymAdditionDutch.Should().Be("DFG");
                    expectedStreetName.HomonymAdditionFrench.Should().Be("XYZ");
                    expectedStreetName.LastEventHash.Should().Be(streetNameHomonymAdditionsWereCorrected.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved_ThenStreetNameHomonymAdditionsWereRemoved()
        {
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

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasMigratedToMunicipality.GetHash() }
            };

            var streetNameHomonymAdditionsWereRemoved = new StreetNameHomonymAdditionsWereRemoved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId),
                new List<Language> {Language.Dutch} );
            ((ISetProvenance)streetNameHomonymAdditionsWereRemoved).SetProvenance(_fixture.Create<Provenance>());

            var streetNameHomonymAdditionsWereRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameHomonymAdditionsWereRemoved.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, metadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereRemoved, streetNameHomonymAdditionsWereRemovedMetadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasMigratedToMunicipality.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                    expectedStreetName.HomonymAdditionDutch.Should().BeNull();
                    expectedStreetName.HomonymAdditionFrench.Should().Be("XYZ");
                    expectedStreetName.LastEventHash.Should().Be(streetNameHomonymAdditionsWereRemoved.GetHash());
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2_ThenStreetNameIsRemoved()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() }
            };

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasApproved).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasApprovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasApproved.GetHash() }
            };

            var streetNameWasRemovedV2 = new StreetNameWasRemovedV2(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId));
            ((ISetProvenance)streetNameWasRemovedV2).SetProvenance(_fixture.Create<Provenance>());
            var streetNameWasRetiredV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasRemovedV2.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, streetNameWasProposedV2Metadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, streetNameWasApprovedMetadata)),
                    new Envelope<StreetNameWasRemovedV2>(new Envelope(streetNameWasRemovedV2, streetNameWasRetiredV2Metadata)))
                .Then(async ct =>
                {
                    var expectedStreetName = (await ct.FindAsync<StreetNameDetailV2>(streetNameWasProposedV2.PersistentLocalId));
                    expectedStreetName.Should().NotBeNull();
                    expectedStreetName.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                    expectedStreetName.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedStreetName.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expectedStreetName.Removed.Should().BeTrue();
                    expectedStreetName.Status.Should().Be(StreetNameStatus.Current);
                    expectedStreetName.VersionTimestamp.Should().Be(streetNameWasRemovedV2.Provenance.Timestamp);
                    expectedStreetName.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expectedStreetName.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expectedStreetName.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expectedStreetName.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                    expectedStreetName.LastEventHash.Should().Be(streetNameWasRemovedV2.GetHash());
                });
        }

        private string DetermineExpectedNameForLanguage(IEnumerable<StreetNameName> streetNameNames, Language language)
        {
            return streetNameNames.SingleOrDefault(x => x.Language == language)?.Name;
        }
    }
}
