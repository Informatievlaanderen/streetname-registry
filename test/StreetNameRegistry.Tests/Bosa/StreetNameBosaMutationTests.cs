namespace StreetNameRegistry.Tests.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using NodaTime;
    using Projections.Bosa;
    using Projections.Bosa.Infrastructure;
    using Xunit;

    public class StreetNameBosaMutationTests : BosaProjectionTest<StreetNameBosaProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/straatnaam";

        // Day 2026-10-21 runs from 2026-10-20T20:00Z until 2026-10-21T20:00Z.
        private const string ChangesFrom = "2026-10-20T20:00:00Z";
        private static readonly DateOnly Day = new DateOnly(2026, 10, 21);
        private static readonly Instant BeforeChangesFrom = Instant.FromUtc(2026, 10, 20, 10, 0);
        private static readonly Instant Morning = Instant.FromUtc(2026, 10, 21, 8, 0);
        private static readonly Instant Noon = Instant.FromUtc(2026, 10, 21, 10, 0);

        private const string BeforeChangesFromVersion = "2026-10-20T12:00:00+02:00";
        private const string MorningVersion = "2026-10-21T10:00:00+02:00";
        private const string NoonVersion = "2026-10-21T12:00:00+02:00";

        private static readonly XNamespace Mutations = StreetNameMutationXml.Mutations;
        private static readonly XNamespace Common = StreetNameMutationXml.Common;

        private readonly Fixture _fixture;
        private long _position;

        public StreetNameBosaMutationTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenStreetNameIsProposed_ThenItIsAnAddWithoutPredecessor()
        {
            await Sut
                .Given(Proposed(1, Morning))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);
                    updates.Should().BeEmpty();

                    var add = adds.Should().ContainSingle().Subject;
                    var streetName = add.Element(Mutations + "streetName")!;
                    streetName.Attribute("beginLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    streetName.Attribute("endLifeSpanVersion").Should().BeNull();
                    Code(streetName).Should().Be(("1", MorningVersion));
                    Status(streetName).Should().Be("proposed");
                    add.Elements(Mutations + "predecessor").Should().BeEmpty();

                    var municipality = streetName.Element(Common + "isAssignedByMunicipality")!;
                    var nisCode = municipality.Element(Common + "objectIdentifier")!.Value;
                    municipality.Element(Common + "versionIdentifier")!.Value.Should().Be($"{{{{municipality:{nisCode}}}}}");
                });
        }

        [Fact]
        public async Task WhenStreetNameChangesBeforeChangesFrom_ThenOnlyStateIsKept()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom))
                .Then(async ct =>
                {
                    (await ct.StreetNames.FindAsync(1))!.VersionId.Should().Be(BeforeChangesFrom);
                    (await ct.StreetNameChanges.AnyAsync()).Should().BeFalse();
                });
        }

        [Fact]
        public async Task WhenNewVersion_ThenPreviousVersionIsEndedAndNewVersionIsAdded()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom, StreetNameStatus.Proposed), Approved(1, Morning))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);

                    var update = updates.Should().ContainSingle().Subject;
                    var previous = update.Element(Mutations + "streetName")!;
                    Code(previous).Should().Be(("1", BeforeChangesFromVersion));
                    previous.Attribute("endLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(previous).Should().Be("proposed");
                    Links(update, "successor").Should().Equal(("1", MorningVersion));

                    var add = adds.Should().ContainSingle().Subject;
                    var current = add.Element(Mutations + "streetName")!;
                    Code(current).Should().Be(("1", MorningVersion));
                    current.Attribute("beginLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(current).Should().Be("current");
                    Links(add, "predecessor").Should().Equal(("1", BeforeChangesFromVersion));
                });
        }

        [Fact]
        public async Task WhenSeveralNewVersionsOnOneDay_ThenTheyAreCollapsed()
        {
            await Sut
                .Given(
                    Migrated(1, BeforeChangesFrom, StreetNameStatus.Proposed),
                    Approved(1, Morning),
                    NamesChanged(1, "Nieuwe straat", Noon))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);

                    var update = updates.Should().ContainSingle().Subject;
                    update.Element(Mutations + "streetName")!.Attribute("endLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Links(update, "successor").Should().Equal(("1", NoonVersion));

                    var add = adds.Should().ContainSingle().Subject;
                    var current = add.Element(Mutations + "streetName")!;
                    Code(current).Should().Be(("1", NoonVersion));
                    current.Attribute("beginLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(current).Should().Be("current");
                    Name(current).Should().Be("Nieuwe straat");
                    add.Elements(Mutations + "event").Should().HaveCount(2);
                    Links(add, "predecessor").Should().Equal(("1", BeforeChangesFromVersion));
                });
        }

        [Fact]
        public async Task WhenStreetNameIsRejected_ThenNewVersionIsRejected()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom, StreetNameStatus.Proposed), Rejected(1, Morning))
                .Then(async ct =>
                {
                    var store = new InMemoryBosaFileStore();
                    await Publisher(store).PublishAsync(ct, new[] { Day }, CancellationToken.None);

                    var add = XDocument.Parse(Encoding.UTF8.GetString(store.Files["add.xml"]));
                    Status(add.Root!.Element(Mutations + "addStreetNameVersion")!.Element(Mutations + "streetName")!).Should().Be("rejected");
                    BosaXsd.ValidateMutations(add, null).Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenCorrected_ThenItIsANewVersion()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom), NamesCorrected(1, "Verbeterde straat", Morning))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);

                    var update = updates.Should().ContainSingle().Subject;
                    var previous = update.Element(Mutations + "streetName")!;
                    Code(previous).Should().Be(("1", BeforeChangesFromVersion));
                    previous.Attribute("endLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Name(previous).Should().Be("Straat");
                    Links(update, "successor").Should().Equal(("1", MorningVersion));

                    var add = adds.Should().ContainSingle().Subject;
                    var current = add.Element(Mutations + "streetName")!;
                    Code(current).Should().Be(("1", MorningVersion));
                    current.Attribute("beginLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Name(current).Should().Be("Verbeterde straat");
                    Links(add, "predecessor").Should().Equal(("1", BeforeChangesFromVersion));
                });
        }

        [Fact]
        public async Task WhenCorrectedAndApprovedOnOneDay_ThenTheyAreCollapsedInOneNewVersion()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom, StreetNameStatus.Proposed), NamesCorrected(1, "Verbeterde straat", Morning), Approved(1, Noon))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);

                    var update = updates.Should().ContainSingle().Subject;
                    var previous = update.Element(Mutations + "streetName")!;
                    Code(previous).Should().Be(("1", BeforeChangesFromVersion));
                    Name(previous).Should().Be("Straat");
                    Status(previous).Should().Be("proposed");
                    Links(update, "successor").Should().Equal(("1", NoonVersion));

                    var add = adds.Should().ContainSingle().Subject;
                    var current = add.Element(Mutations + "streetName")!;
                    Code(current).Should().Be(("1", NoonVersion));
                    Name(current).Should().Be("Verbeterde straat");
                    Status(current).Should().Be("current");
                    add.Elements(Mutations + "event").Should().HaveCount(2);
                    Links(add, "predecessor").Should().Equal(("1", BeforeChangesFromVersion));

                    var store = new InMemoryBosaFileStore();
                    await Publisher(store).PublishAsync(ct, new[] { Day }, CancellationToken.None);
                    BosaXsd.ValidateMutations(
                        XDocument.Parse(Encoding.UTF8.GetString(store.Files["add.xml"])),
                        XDocument.Parse(Encoding.UTF8.GetString(store.Files["update.xml"]))).Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenStreetNameIsProposedAndChangedOnTheSameDay_ThenItIsOneAdd()
        {
            await Sut
                .Given(Proposed(1, Morning), Approved(1, Noon))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);
                    updates.Should().BeEmpty();

                    var current = adds.Should().ContainSingle().Subject.Element(Mutations + "streetName")!;
                    Code(current).Should().Be(("1", NoonVersion));
                    current.Attribute("beginLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(current).Should().Be("current");
                });
        }

        [Fact]
        public async Task WhenStreetNameIsRemoved_ThenItsVersionIsEndedWithItsState()
        {
            await Sut
                .Given(Migrated(1, BeforeChangesFrom, StreetNameStatus.Current), Removed(1, Morning))
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);
                    adds.Should().BeEmpty();

                    var update = updates.Should().ContainSingle().Subject;
                    var streetName = update.Element(Mutations + "streetName")!;
                    Code(streetName).Should().Be(("1", BeforeChangesFromVersion));
                    streetName.Attribute("endLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(streetName).Should().Be("current");
                    update.Elements(Mutations + "successor").Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenStreetNamesAreMerged_ThenVersionsAreLinked()
        {
            await Sut
                .Given(MergerEvents())
                .Then(async ct =>
                {
                    var (adds, updates) = await Publish(ct);

                    var add = adds.Should().ContainSingle().Subject;
                    Code(add.Element(Mutations + "streetName")!).Should().Be(("2", MorningVersion));
                    add.Element(Mutations + "event")!.Element(Mutations + "type")!.Value.Should().Be("streetMerge");
                    Links(add, "predecessor").Should().Equal(("1", BeforeChangesFromVersion));

                    var update = updates.Should().ContainSingle().Subject;
                    var ended = update.Element(Mutations + "streetName")!;
                    Code(ended).Should().Be(("1", BeforeChangesFromVersion));
                    ended.Attribute("endLifeSpanVersion")!.Value.Should().Be("2026-10-21T08:00:00Z");
                    Status(ended).Should().Be("current");
                    update.Element(Mutations + "event")!.Element(Mutations + "type")!.Value.Should().Be("streetMerge");
                    Links(update, "successor").Should().Equal(("2", MorningVersion));
                });
        }

        [Fact]
        public async Task DayFilesAreValidAgainstXsd()
        {
            var events = MergerEvents().Concat(new[] { Approved(2, Noon), NamesCorrected(2, "Verbeterde straat", Noon) }).ToArray();

            await Sut
                .Given(events)
                .Then(async ct =>
                {
                    var store = new InMemoryBosaFileStore();
                    await Publisher(store).PublishAsync(ct, new[] { Day }, CancellationToken.None);

                    XDocument File(string name) => XDocument.Parse(Encoding.UTF8.GetString(store.Files[name]));

                    var add = File("add.xml");
                    var update = File("update.xml");
                    add.Root!.HasElements.Should().BeTrue();
                    update.Root!.HasElements.Should().BeTrue();
                    BosaXsd.ValidateMutations(add, update).Should().BeEmpty();
                });
        }

        private object[] MergerEvents()
        {
            var migrated = At(new StreetNameWasMigratedToMunicipalityBuilder(_fixture)
                .WithPersistentLocalId(1)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names { new StreetNameName("Oude straat", Language.Dutch), new StreetNameName("Old street", Language.English) })
                .WithHomonymAdditions(new HomonymAdditions(new[] { new StreetNameHomonymAddition("ABC", Language.Dutch) }))
                .Build(), BeforeChangesFrom);
            var proposedForMerger = At(new StreetNameWasProposedForMunicipalityMergerBuilder(_fixture)
                .WithPersistentLocalId(2)
                .WithNames(new Names { new StreetNameName("Merge straat", Language.Dutch) })
                .WithMergedStreetNamePersistentLocalIds(new List<PersistentLocalId> { new PersistentLocalId(1) })
                .Build(), Morning);
            var retiredBecauseOfMerger = At(new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                new MunicipalityId(migrated.MunicipalityId),
                new PersistentLocalId(1),
                new[] { new PersistentLocalId(2) }), Morning);

            return new[] { Wrap(migrated), Wrap(proposedForMerger), Wrap(retiredBecauseOfMerger) };
        }

        private object Migrated(int id, Instant at, StreetNameStatus status = StreetNameStatus.Current)
            => Wrap(At(new StreetNameWasMigratedToMunicipalityBuilder(_fixture)
                .WithPersistentLocalId(id)
                .WithStatus(status)
                .WithNames(new Names { new StreetNameName("Straat", Language.Dutch) })
                .Build(), at));

        private object Proposed(int id, Instant at)
            => Wrap(At(new StreetNameWasProposedV2Builder(_fixture)
                .WithPersistentLocalId(id)
                .WithNames(new Names { new StreetNameName("Straat", Language.Dutch) })
                .Build(), at));

        private object Approved(int id, Instant at)
            => Wrap(At(new StreetNameWasApprovedBuilder(_fixture).WithPersistentLocalId(id).Build(), at));

        private object Rejected(int id, Instant at)
            => Wrap(At(new StreetNameWasRejected(_fixture.Create<MunicipalityId>(), new PersistentLocalId(id)), at));

        private object NamesChanged(int id, string name, Instant at)
            => Wrap(At(new StreetNameNamesWereChanged(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(id),
                new Names { new StreetNameName(name, Language.Dutch) }), at));

        private object NamesCorrected(int id, string name, Instant at)
            => Wrap(At(new StreetNameNamesWereCorrected(
                _fixture.Create<MunicipalityId>(),
                new PersistentLocalId(id),
                new Names { new StreetNameName(name, Language.Dutch) }), at));

        private object Removed(int id, Instant at)
            => Wrap(At(new StreetNameWasRemovedV2(_fixture.Create<MunicipalityId>(), new PersistentLocalId(id)), at));

        private static T At<T>(T @event, Instant timestamp) where T : ISetProvenance
        {
            @event.SetProvenance(new Provenance(timestamp, Application.StreetNameRegistry, new Reason(""), new Operator(""), Modification.Update, Organisation.Municipality));
            return @event;
        }

        private object Wrap<T>(T @event) where T : Be.Vlaanderen.Basisregisters.EventHandling.IMessage
            => new Envelope<T>(new Envelope(@event, new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++_position }
            }));

        private static BosaDailyFilePublisher Publisher(IBosaFileStore store)
            => new BosaDailyFilePublisher(new BosaXmlWriter(), store, Options.Create(new BosaOptions { FileNameFormat = "{1}.xml" }));

        private static async Task<(IReadOnlyList<XElement> Adds, IReadOnlyList<XElement> Updates)> Publish(BosaContext context)
        {
            var store = new InMemoryBosaFileStore();
            await Publisher(store).PublishAsync(context, new[] { Day }, CancellationToken.None);

            XElement Root(string name) => XDocument.Parse(Encoding.UTF8.GetString(store.Files[name])).Root!;
            return (Root("add.xml").Elements().ToList(), Root("update.xml").Elements().ToList());
        }

        private static (string Id, string Version) Code(XElement streetName)
        {
            var code = streetName.Element(Common + "code")!;
            return (code.Element(Common + "objectIdentifier")!.Value, code.Element(Common + "versionIdentifier")!.Value);
        }

        private static IEnumerable<(string Id, string Version)> Links(XElement element, string link)
            => element.Elements(Mutations + link)
                .Select(x => (x.Element(Common + "objectIdentifier")!.Value, x.Element(Common + "versionIdentifier")!.Value));

        private static string Status(XElement streetName)
            => streetName.Element(Common + "streetNameStatus")!.Element(Common + "status")!.Value;

        private static string Name(XElement streetName)
            => streetName.Element(Common + "name")!.Element(Common + "spelling")!.Value;

        protected override StreetNameBosaProjections CreateProjection()
            => new StreetNameBosaProjections(
                new OptionsWrapper<BosaOptions>(new BosaOptions { Namespace = Namespace }),
                new BosaDay(new BosaOptions { ChangesFrom = ChangesFrom }));
    }
}
