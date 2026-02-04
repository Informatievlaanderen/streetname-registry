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

    public class StreetNameFeedProjectionsTests
    {
        private readonly Fixture _fixture;

        private ConnectedProjectionTest<FeedContext, StreetNameFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }

        public StreetNameFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            Sut = new ConnectedProjectionTest<FeedContext, StreetNameFeedProjections>(CreateContext,
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
                    document!.RecordCreatedAt.Should().Be(streetNameWasMigrated.Provenance.Timestamp);
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
                                               a.NewValue!.ToString() == nameof(StraatnaamStatus.Voorgesteld))),
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
            var streetNameWasProposedForMunicipalityMerger = _fixture.Create<StreetNameWasProposedForMunicipalityMerger>();

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

                    var feedItem = await context.StreetNameFeed.SingleOrDefaultAsync(x =>
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

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRejected_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRenamed_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved_ThenFeedItemIsAddedAndDocumentUpdated()
        {

        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2_ThenFeedItemIsAddedAndDocumentUpdated()
        {

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

        protected virtual FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}
