namespace StreetNameRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using CloudNative.CloudEvents;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Municipality;
    using Municipality.Events;
    using Newtonsoft.Json;
    using Projections.Feed;
    using Projections.Feed.Contract;
    using Projections.Feed.StreetNameFeed;
    using Xunit;

    public sealed class StreetNameFeedProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;

        private ConnectedProjectionTest<FeedContext, StreetNameFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }

        public StreetNameFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            _feedContext = CreateContext();
            Sut = new ConnectedProjectionTest<FeedContext, StreetNameFeedProjections>(() => _feedContext,
                () => new StreetNameFeedProjections(ChangeFeedServiceMock.Object));

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPersistentLocalId());

            SetupChangeFeedServiceMock();
        }

        [Fact]
        public async Task WhenStreetNameWasMigratedToMunicipality_ThenFeedItemAndDocumentAreAdded()
        {
            var nameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var nameEnglish = new StreetNameName(_fixture.Create<string>(), Language.English);
            var nameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            var nameGerman = new StreetNameName(_fixture.Create<string>(), Language.German);

            var homonymDutch = new StreetNameHomonymAddition("homonymDutch", Language.Dutch);
            var homonymFrench = new StreetNameHomonymAddition("homonymFrench", Language.French);
            var homonymEnglish = new StreetNameHomonymAddition("homonymEnglish", Language.English);
            var homonymGerman = new StreetNameHomonymAddition("homonymGerman", Language.German);
            _fixture.Register(() => new Names([nameDutch, nameEnglish, nameFrench, nameGerman]));
            _fixture.Register(() => new HomonymAdditions([homonymDutch, homonymEnglish, homonymFrench, homonymGerman]));
            var streetNameWasMigrated = _fixture.Create<StreetNameWasMigratedToMunicipality>();

            var position = 2L;

            await Sut
                .Given(_fixture.Create<MunicipalityWasImported>(), CreateEnvelope(streetNameWasMigrated, position))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasMigrated.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.RecordCreatedAt.Should().Be(streetNameWasMigrated.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(streetNameWasMigrated.Provenance.Timestamp);

                    document.Document.PersistentLocalId.Should().Be(streetNameWasMigrated.PersistentLocalId);
                    document.Document.NisCode.Should().Be(streetNameWasMigrated.NisCode);
                    document.Document.Names.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(nameDutch.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.EN).Spelling.Should().Be(nameEnglish.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(nameFrench.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.DE).Spelling.Should().Be(nameGerman.Name);
                    document.Document.Status.Should().Be(StraatnaamStatus.Voorgesteld);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(homonymDutch.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.EN).Spelling.Should().Be(homonymEnglish.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(homonymFrench.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.DE).Spelling.Should().Be(homonymGerman.HomonymAddition);

                    var feedItem = await context.StreetNameFeed.SingleOrDefaultAsync(x => x.PersistentLocalId == streetNameWasMigrated.PersistentLocalId);
                    AssertFeedItem(feedItem, position, streetNameWasMigrated);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.CreateV1,
                            streetNameWasMigrated.PersistentLocalId.ToString(),
                            streetNameWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasMigrated.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.MunicipalityId &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == OsloNamespaces.Gemeente.ToPuri(streetNameWasMigrated.NisCode)) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StreetNameNames &&
                                               a.OldValue == null &&
                                               a.NewValue == document.Document.Names) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.HomonymAdditions &&
                                               a.OldValue == null &&
                                               a.NewValue == document.Document.HomonymAdditions) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == document.Document.Status.ToString())),
                            StreetNameWasMigratedToMunicipality.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposedV2_ThenFeedItemAndDocumentAreAdded()
        {
            var nameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var nameEnglish = new StreetNameName(_fixture.Create<string>(), Language.English);
            var nameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            var nameGerman = new StreetNameName(_fixture.Create<string>(), Language.German);
            _fixture.Register(() => new Names([nameDutch, nameEnglish, nameFrench, nameGerman]));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var position = 2L;

            await Sut
                .Given(_fixture.Create<MunicipalityWasImported>(), CreateEnvelope(streetNameWasProposedV2, position))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.RecordCreatedAt.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);

                    document.Document.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    document.Document.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    document.Document.Names.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(nameDutch.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.EN).Spelling.Should().Be(nameEnglish.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(nameFrench.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.DE).Spelling.Should().Be(nameGerman.Name);
                    document.Document.Status.Should().Be(StraatnaamStatus.Voorgesteld);
                    document.Document.HomonymAdditions.Should().BeEmpty();

                    var feedItem = await context.StreetNameFeed.SingleOrDefaultAsync(x => x.PersistentLocalId == streetNameWasProposedV2.PersistentLocalId);
                    AssertFeedItem(feedItem, position, streetNameWasProposedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.CreateV1,
                            streetNameWasProposedV2.PersistentLocalId.ToString(),
                            streetNameWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.MunicipalityId &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == OsloNamespaces.Gemeente.ToPuri(streetNameWasProposedV2.NisCode)) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StreetNameNames &&
                                               a.OldValue == null &&
                                               a.NewValue == document.Document.Names) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld))),
                            StreetNameWasProposedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.InGebruik);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasApproved.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameWasApproved);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasApproved.PersistentLocalId.ToString(),
                            streetNameWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.InGebruik))),
                            StreetNameWasApproved.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposedForMunicipalityMerger_ThenFeedItemAndDocumentAreAdded()
        {
            var nameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var nameEnglish = new StreetNameName(_fixture.Create<string>(), Language.English);
            var nameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            var nameGerman = new StreetNameName(_fixture.Create<string>(), Language.German);
            var homonymDutch = new StreetNameHomonymAddition("homonymDutch", Language.Dutch);
            var homonymFrench = new StreetNameHomonymAddition("homonymFrench", Language.French);
            var homonymEnglish = new StreetNameHomonymAddition("homonymEnglish", Language.English);
            var homonymGerman = new StreetNameHomonymAddition("homonymGerman", Language.German);
            _fixture.Register(() => new Names([nameDutch, nameEnglish, nameFrench, nameGerman]));
            _fixture.Register(() => new HomonymAdditions([homonymDutch, homonymEnglish, homonymFrench, homonymGerman]));
            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMerger(
                _fixture.Create<MunicipalityId>(),
                _fixture.Create<NisCode>(),
                _fixture.Create<StreetNameStatus>(),
                _fixture.Create<Names>(),
                _fixture.Create<HomonymAdditions>(),
                _fixture.Create<PersistentLocalId>(),
                [new PersistentLocalId(_fixture.Create<int>()), new PersistentLocalId(_fixture.Create<int>())]);
            ((ISetProvenance)streetNameWasProposedForMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            _feedContext.StreetNameDocuments.Add(
                new StreetNameDocument(
                    streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.First(),
                    "11001",
                    streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp,
                    _fixture.CreateMany<GeografischeNaam>(2).ToList()));

            _feedContext.StreetNameDocuments.Add(
                new StreetNameDocument(
                    streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.LastOrDefault(),
                    "11002",
                    streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp,
                    _fixture.CreateMany<GeografischeNaam>(2).ToList()));

            var position = 2L;

            await Sut
                .Given(_fixture.Create<MunicipalityWasImported>(), CreateEnvelope(streetNameWasProposedForMunicipalityMerger, position))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.RecordCreatedAt.Should().Be(streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp);

                    document.Document.PersistentLocalId.Should().Be(streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    document.Document.NisCode.Should().Be(streetNameWasProposedForMunicipalityMerger.NisCode);
                    document.Document.Names.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(nameDutch.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.EN).Spelling.Should().Be(nameEnglish.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(nameFrench.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.DE).Spelling.Should().Be(nameGerman.Name);
                    document.Document.Status.Should().Be(StraatnaamStatus.Voorgesteld);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(homonymDutch.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.EN).Spelling.Should().Be(homonymEnglish.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(homonymFrench.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.DE).Spelling.Should().Be(homonymGerman.HomonymAddition);

                    var feedItem = await context.StreetNameFeed.FirstOrDefaultAsync(x =>
                        x.PersistentLocalId == streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    AssertFeedItem(feedItem, position, streetNameWasProposedForMunicipalityMerger);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.CreateV1,
                            streetNameWasProposedForMunicipalityMerger.PersistentLocalId.ToString(),
                            streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedForMunicipalityMerger.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.MunicipalityId &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == OsloNamespaces.Gemeente.ToPuri(streetNameWasProposedForMunicipalityMerger.NisCode)) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StreetNameNames &&
                                               a.OldValue == null &&
                                               a.NewValue == document.Document.Names) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.HomonymAdditions &&
                                               a.OldValue == null &&
                                               a.NewValue == document.Document.HomonymAdditions) &&
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName &&
                                               a.OldValue == null &&
                                               a.NewValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld))),
                            StreetNameWasProposedForMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    // We will only create a transform event for the merged street names, not for the proposed street name for municipality merger itself, as the proposed street name for municipality merger is not really a transformation of the merged street names,
                    // but rather a new street name that is proposed in the context of a municipality merger.
                    // The merged street names will be transformed to have a new persistent local id and be linked to the proposed street name for municipality merger as part of the transformation process, but the proposed street name for municipality merger itself is not a transformation of the merged street names.
                    // ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                    //         It.IsAny<long>(),
                    //         streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                    //         StreetNameEventTypes.TransformV1,
                    //         It.Is<StreetNameCloudTransformEvent>(e => e.NisCodes.SequenceEqual(new List<string> { "11001", "11002", streetNameWasProposedForMunicipalityMerger.NisCode })
                    //                                                   && e.To.SequenceEqual(new List<string>{OsloNamespaces.StraatNaam.ToPuri(streetNameWasProposedForMunicipalityMerger.PersistentLocalId.ToString())})
                    //                                                   && e.From == OsloNamespaces.StraatNaam.ToPuri(streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.First().ToString())),
                    //         It.IsAny<Uri>(),
                    //         StreetNameWasProposedForMunicipalityMerger.EventName,
                    //         It.IsAny<string>()),
                    //     Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(1));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(1));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();
            var streetNameWasCorrectedFromApprovedToProposed = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposed>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1),
                    CreateEnvelope(streetNameWasCorrectedFromApprovedToProposed, position + 2))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Voorgesteld);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 2, streetNameWasCorrectedFromApprovedToProposed);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId.ToString(),
                            streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld))),
                            StreetNameWasCorrectedFromApprovedToProposed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejected = _fixture.Create<StreetNameWasRejected>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasRejected, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Afgekeurd);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRejected.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameWasRejected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasRejected.PersistentLocalId.ToString(),
                            streetNameWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Afgekeurd))),
                            StreetNameWasRejected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejectedBecauseOfMunicipalityMerger = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                new MunicipalityId(streetNameWasProposedV2.MunicipalityId),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new List<PersistentLocalId> { new PersistentLocalId(_fixture.Create<int>()) });
            ((ISetProvenance)streetNameWasRejectedBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            _feedContext.StreetNameDocuments.Add(
                new StreetNameDocument(
                    streetNameWasRejectedBecauseOfMunicipalityMerger.NewPersistentLocalIds.First(),
                    "11001",
                    streetNameWasProposedV2.Provenance.Timestamp,
                    _fixture.CreateMany<GeografischeNaam>(2).ToList()));
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasRejectedBecauseOfMunicipalityMerger, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Afgekeurd);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRejectedBecauseOfMunicipalityMerger.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameWasRejectedBecauseOfMunicipalityMerger);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasRejectedBecauseOfMunicipalityMerger.PersistentLocalId.ToString(),
                            streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Afgekeurd))),
                            StreetNameWasRejectedBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                        It.IsAny<long>(),
                        streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                        StreetNameEventTypes.TransformV1,
                        It.Is<StreetNameCloudTransformEvent>(e => e.NisCodes.SequenceEqual(new List<string> { "11001", streetNameWasProposedV2.NisCode })
                        && e.From == OsloNamespaces.StraatNaam.ToPuri(streetNameWasProposedV2.PersistentLocalId.ToString())
                        && e.To.SequenceEqual(streetNameWasRejectedBecauseOfMunicipalityMerger.NewPersistentLocalIds.Select(s => OsloNamespaces.StraatNaam.ToPuri(s.ToString())).ToList())),
                        It.IsAny<Uri>(),
                        StreetNameWasRejectedBecauseOfMunicipalityMerger.EventName,
                        It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejected = _fixture.Create<StreetNameWasRejected>();
            var streetNameWasCorrectedFromRejectedToProposed = _fixture.Create<StreetNameWasCorrectedFromRejectedToProposed>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasRejected, position + 1),
                    CreateEnvelope(streetNameWasCorrectedFromRejectedToProposed, position + 2))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Voorgesteld);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 2, streetNameWasCorrectedFromRejectedToProposed);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId.ToString(),
                            streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.Afgekeurd)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld))),
                            StreetNameWasCorrectedFromRejectedToProposed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredV2 = _fixture.Create<StreetNameWasRetiredV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1),
                    CreateEnvelope(streetNameWasRetiredV2, position + 2))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Gehistoreerd);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRetiredV2.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 2, streetNameWasRetiredV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRetiredV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasRetiredV2.PersistentLocalId.ToString(),
                            streetNameWasRetiredV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Gehistoreerd))),
                            StreetNameWasRetiredV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredBecauseOfMunicipalityMerger = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                new MunicipalityId(streetNameWasProposedV2.MunicipalityId),
                new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                new List<PersistentLocalId> { new PersistentLocalId(_fixture.Create<int>()) });
            ((ISetProvenance)streetNameWasRetiredBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            _feedContext.StreetNameDocuments.Add(
                new StreetNameDocument(
                    streetNameWasRetiredBecauseOfMunicipalityMerger.NewPersistentLocalIds.First(),
                    "11001",
                    streetNameWasProposedV2.Provenance.Timestamp,
                    _fixture.CreateMany<GeografischeNaam>(2).ToList()));

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1),
                    CreateEnvelope(streetNameWasRetiredBecauseOfMunicipalityMerger, position + 2))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Gehistoreerd);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 2, streetNameWasRetiredBecauseOfMunicipalityMerger);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId.ToString(),
                            streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Gehistoreerd))),
                            StreetNameWasRetiredBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.TransformV1,
                            It.Is<StreetNameCloudTransformEvent>(e => e.NisCodes.SequenceEqual(new List<string> { "11001", streetNameWasProposedV2.NisCode })
                                                                      && e.From == OsloNamespaces.StraatNaam.ToPuri(streetNameWasProposedV2.PersistentLocalId.ToString())
                                                                      && e.To.SequenceEqual(streetNameWasRetiredBecauseOfMunicipalityMerger.NewPersistentLocalIds.Select(s => OsloNamespaces.StraatNaam.ToPuri(s.ToString())).ToList())),
                            It.IsAny<Uri>(),
                            StreetNameWasRetiredBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(4));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();
            var streetNameWasRenamed = _fixture.Create<StreetNameWasRenamed>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1),
                    CreateEnvelope(streetNameWasRenamed, position + 2))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.Gehistoreerd);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRenamed.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 2, streetNameWasRenamed);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRenamed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasRenamed.PersistentLocalId.ToString(),
                            streetNameWasRenamed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.Gehistoreerd))),
                            StreetNameWasRenamed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            streetNameWasRenamed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.TransformV1,
                            It.Is<StreetNameCloudTransformEvent>(e => e.NisCodes.SequenceEqual(new List<string> { streetNameWasProposedV2.NisCode })
                                                                      && e.From == OsloNamespaces.StraatNaam.ToPuri(streetNameWasProposedV2.PersistentLocalId.ToString())
                                                                      && e.To.SequenceEqual(new List<string>{OsloNamespaces.StraatNaam.ToPuri(streetNameWasRenamed.DestinationPersistentLocalId.ToString())})),
                            It.IsAny<Uri>(),
                            StreetNameWasRenamed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(4));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredV2 = _fixture.Create<StreetNameWasRetiredV2>();
            var streetNameWasCorrectedFromRetiredToCurrent = _fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasApproved, position + 1),
                    CreateEnvelope(streetNameWasRetiredV2, position + 2),
                    CreateEnvelope(streetNameWasCorrectedFromRetiredToCurrent, position + 3))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                    document.Document.Status.Should().Be(StraatnaamStatus.InGebruik);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 3, streetNameWasCorrectedFromRetiredToCurrent);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId.ToString(),
                            streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(StraatnaamStatus.Gehistoreerd)
                                               && a.NewValue!.ToString() == nameof(StraatnaamStatus.InGebruik))),
                            StreetNameWasCorrectedFromRetiredToCurrent.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(4));
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            var initialNameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var initialNameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            _fixture.Register(() => new Names([initialNameDutch, initialNameFrench]));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var correctedNameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var correctedNameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            _fixture.Register(() => new Names([correctedNameDutch, correctedNameFrench]));
            var streetNameNamesWereCorrected = _fixture.Create<StreetNameNamesWereCorrected>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameNamesWereCorrected, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                    document.Document.Names.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(correctedNameDutch.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(correctedNameFrench.Name);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameNamesWereCorrected.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameNamesWereCorrected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameNamesWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameNamesWereCorrected.PersistentLocalId.ToString(),
                            streetNameNamesWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StreetNameNames &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.NL && n.Spelling == initialNameDutch.Name) &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.FR && n.Spelling == initialNameFrench.Name) &&
                                               a.NewValue == document.Document.Names)),
                            StreetNameNamesWereCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            var initialNameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var initialNameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            _fixture.Register(() => new Names([initialNameDutch, initialNameFrench]));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var changedNameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var changedNameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            _fixture.Register(() => new Names([changedNameDutch, changedNameFrench]));
            var streetNameNamesWereChanged = _fixture.Create<StreetNameNamesWereChanged>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameNamesWereChanged, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                    document.Document.Names.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(changedNameDutch.Name);
                    document.Document.Names.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(changedNameFrench.Name);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameNamesWereChanged.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameNamesWereChanged);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameNamesWereChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameNamesWereChanged.PersistentLocalId.ToString(),
                            streetNameNamesWereChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.StreetNameNames &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.NL && n.Spelling == initialNameDutch.Name) &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.FR && n.Spelling == initialNameFrench.Name) &&
                                               a.NewValue == document.Document.Names)),
                            StreetNameNamesWereChanged.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            var nameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var nameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            var initialHomonymDutch = new StreetNameHomonymAddition("initialDutch", Language.Dutch);
            var initialHomonymFrench = new StreetNameHomonymAddition("initialFrench", Language.French);
            _fixture.Register(() => new Names([nameDutch, nameFrench]));
            _fixture.Register(() => new HomonymAdditions([initialHomonymDutch, initialHomonymFrench]));
            var streetNameWasMigrated = _fixture.Create<StreetNameWasMigratedToMunicipality>();

            var correctedHomonymDutch = new StreetNameHomonymAddition("correctedDutch", Language.Dutch);
            var correctedHomonymFrench = new StreetNameHomonymAddition("correctedFrench", Language.French);
            _fixture.Register(() => new List<StreetNameHomonymAddition>([correctedHomonymDutch, correctedHomonymFrench]));
            var streetNameHomonymAdditionsWereCorrected = _fixture.Create<StreetNameHomonymAdditionsWereCorrected>();

            var position = 2L;

            await Sut
                .Given(_fixture.Create<MunicipalityWasImported>(),
                    CreateEnvelope(streetNameWasMigrated, position),
                    CreateEnvelope(streetNameHomonymAdditionsWereCorrected, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasMigrated.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.NL).Spelling.Should().Be(correctedHomonymDutch.HomonymAddition);
                    document.Document.HomonymAdditions.Single(x => x.Taal == Taal.FR).Spelling.Should().Be(correctedHomonymFrench.HomonymAddition);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameHomonymAdditionsWereCorrected.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameHomonymAdditionsWereCorrected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameHomonymAdditionsWereCorrected.PersistentLocalId.ToString(),
                            streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasMigrated.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.HomonymAdditions &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.NL && n.Spelling == initialHomonymDutch.HomonymAddition) &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.FR && n.Spelling == initialHomonymFrench.HomonymAddition) &&
                                               a.NewValue == document.Document.HomonymAdditions)),
                            StreetNameHomonymAdditionsWereCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            var nameDutch = new StreetNameName(_fixture.Create<string>(), Language.Dutch);
            var nameFrench = new StreetNameName(_fixture.Create<string>(), Language.French);
            var initialHomonymDutch = new StreetNameHomonymAddition("initialHomonymDutch", Language.Dutch);
            var initialHomonymFrench = new StreetNameHomonymAddition("initialHomonymFrench", Language.French);
            _fixture.Register(() => new Names([nameDutch, nameFrench]));
            _fixture.Register(() => new HomonymAdditions([initialHomonymDutch, initialHomonymFrench]));
            var streetNameWasMigrated = _fixture.Create<StreetNameWasMigratedToMunicipality>();

            _fixture.Register(() => new List<Language> { Language.French });
            var streetNameHomonymAdditionsWereRemoved = _fixture.Create<StreetNameHomonymAdditionsWereRemoved>();

            var position = 2L;

            await Sut
                .Given(_fixture.Create<MunicipalityWasImported>(),
                    CreateEnvelope(streetNameWasMigrated, position),
                    CreateEnvelope(streetNameHomonymAdditionsWereRemoved, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasMigrated.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp);
                    document.Document.HomonymAdditions.Should().Contain(x => x.Taal == Taal.NL && x.Spelling == initialHomonymDutch.HomonymAddition);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameHomonymAdditionsWereRemoved.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameHomonymAdditionsWereRemoved);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.UpdateV1,
                            streetNameHomonymAdditionsWereRemoved.PersistentLocalId.ToString(),
                            streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasMigrated.NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == StreetNameAttributeNames.HomonymAdditions &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.NL && n.Spelling == initialHomonymDutch.HomonymAddition) &&
                                               ((List<GeografischeNaam>)a.OldValue).Any(n => n.Taal == Taal.FR && n.Spelling == initialHomonymFrench.HomonymAddition) &&
                                               a.NewValue == document.Document.HomonymAdditions)),
                            StreetNameHomonymAdditionsWereRemoved.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2_ThenFeedItemIsAddedAndDocumentUpdated()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRemovedV2 = _fixture.Create<StreetNameWasRemovedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(streetNameWasProposedV2, position),
                    CreateEnvelope(streetNameWasRemovedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.StreetNameDocuments.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeTrue();
                    document.LastChangedOn.Should().Be(streetNameWasRemovedV2.Provenance.Timestamp);

                    var feedItem = await context.StreetNameFeed.LastAsync(x => x.PersistentLocalId == streetNameWasRemovedV2.PersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, streetNameWasRemovedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            StreetNameEventTypes.DeleteV1,
                            streetNameWasRemovedV2.PersistentLocalId.ToString(),
                            streetNameWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(streetNameWasProposedV2.NisCode)),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            StreetNameWasRemovedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        private static void AssertFeedItem(
            StreetNameFeedItem? feedItem,
            long position,
            IMunicipalityEvent @event)
        {
            feedItem.Should().NotBeNull();
            feedItem!.CloudEventAsString.Should().NotBeNullOrEmpty();
            feedItem.Page.Should().Be(1);
            feedItem.Position.Should().Be(position);
            feedItem.Application.Should().Be(@event.Provenance.Application);
            feedItem.Modification.Should().Be(@event.Provenance.Modification);
            feedItem.Operator.Should().Be(@event.Provenance.Operator);
            feedItem.Organisation.Should().Be(@event.Provenance.Organisation);
            feedItem.Reason.Should().Be(@event.Provenance.Reason);
        }


        private Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() }
            };
            return new Envelope<T>(new Envelope(@event, metadata));
        }

        private void SetupChangeFeedServiceMock()
        {
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEventWithData(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new CloudEvent());

            ChangeFeedServiceMock.Setup(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>())).Returns("serialized cloud event");

            ChangeFeedServiceMock.Setup(x => x.CheckToUpdateCacheAsync(
                It.IsAny<int>(),
                It.IsAny<FeedContext>(),
                It.IsAny<Func<int, Task<int>>>()));
        }

        private FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}
