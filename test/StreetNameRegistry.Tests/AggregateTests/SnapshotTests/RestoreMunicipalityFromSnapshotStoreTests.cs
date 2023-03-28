namespace StreetNameRegistry.Tests.AggregateTests.SnapshotTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.DataStructures;
    using Municipality.Events;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class RestoreMunicipalityFromSnapshotStoreTests : StreetNameRegistryTest
    {
        private readonly Municipality _sut;
        private readonly MunicipalitySnapshot _municipalitySnapshot;

        public RestoreMunicipalityFromSnapshotStoreTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var random = new Random(Fixture.Create<int>());

            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithMunicipalityStatus());
            Fixture.Customize(new WithValidHomonymAddition());

            Fixture.Register<Fixture, MunicipalityStreetNames>(fixture =>
            {
                var municipalityStreetNames = new MunicipalityStreetNames();
                var streetNameData = fixture
                    .Build<StreetNameData>()
                    .FromFactory(() => new StreetNameData(
                        fixture.Create<PersistentLocalId>(),
                        fixture.Create<StreetNameStatus>(),
                        new Names(fixture.Create<IDictionary<Language, string>>()),
                        new HomonymAdditions(fixture.Create<List<StreetNameHomonymAddition>>()),
                        fixture.Create<bool>(),
                        fixture.Create<StreetNameId?>(),
                        fixture.Create<string>(),
                        fixture.Create<ProvenanceData>()))
                    .CreateMany(random.Next(2, 10));

                var streetNames = fixture.Build<IEnumerable<MunicipalityStreetName>>().FromFactory(() =>
                {
                    var names = new List<MunicipalityStreetName>();

                    foreach (var data in streetNameData)
                    {
                        var municipalityStreetName = new MunicipalityStreetName(o => { });
                        municipalityStreetName.RestoreSnapshot(Fixture.Create<MunicipalityId>(), data);

                        names.Add(municipalityStreetName);
                    }

                    return names;
                }).Create().ToList();

                municipalityStreetNames.AddRange(streetNames);

                return municipalityStreetNames;
            });

            _sut = new MunicipalityFactory(IntervalStrategy.Default).Create();
            _municipalitySnapshot = Fixture.Create<MunicipalitySnapshot>();

            var eventSerializer = Container.Resolve<EventSerializer>();
            var eventMapping = Container.Resolve<EventMapping>();

            var streamId = new MunicipalityStreamId(Fixture.Create<MunicipalityId>());
            Container.Resolve<ISnapshotStore>().SaveSnapshotAsync(streamId,
                new SnapshotContainer
                {
                    Data = eventSerializer.SerializeObject(_municipalitySnapshot),
                    Info = new SnapshotInfo
                    {
                        StreamVersion = 1,
                        Type = eventMapping.GetEventName(_municipalitySnapshot.GetType()),
                    }
                },
                CancellationToken.None);

            Container.Resolve<IStreamStore>().AppendToStream(new StreamId(streamId), ExpectedVersion.NoStream, Fixture.Create<NewStreamMessage>());

            _sut = Container.Resolve<IMunicipalities>().GetAsync(streamId, CancellationToken.None).GetAwaiter().GetResult();
        }

        [Fact]
        public void ThenAggregateMunicipalityStateIsExpected()
        {
            _sut.MunicipalityId.Should().Be(new MunicipalityId(_municipalitySnapshot.MunicipalityId));
            _sut.NisCode.Should().Be(new NisCode(_municipalitySnapshot.NisCode));
            _sut.MunicipalityStatus.Should().Be(MunicipalityStatus.Parse(_municipalitySnapshot.MunicipalityStatus));
            _sut.OfficialLanguages.Should().BeEquivalentTo(_municipalitySnapshot.OfficialLanguages);
            _sut.FacilityLanguages.Should().BeEquivalentTo(_municipalitySnapshot.FacilityLanguages);
        }

        [Fact]
        public void ThenAggregateStreetNamesStateAreExpected()
        {
            _sut.StreetNames.Should().NotBeEmpty();
            foreach (var streetName in _sut.StreetNames)
            {
                var snapshotStreetName = _municipalitySnapshot.StreetNames.SingleOrDefault(x => x.StreetNamePersistentLocalId == streetName.PersistentLocalId);

                snapshotStreetName.Should().NotBeNull();

                streetName.Status.Should().Be(snapshotStreetName!.Status);
                streetName.IsRemoved.Should().Be(snapshotStreetName.IsRemoved);
                streetName.Names.Should().BeEquivalentTo(new Names(snapshotStreetName.Names));
                streetName.HomonymAdditions.Should().BeEquivalentTo(new HomonymAdditions(snapshotStreetName.HomonymAdditions));

                if (snapshotStreetName.LegacyStreetNameId is null)
                {
                    streetName.LegacyStreetNameId.Should().BeNull();
                }
                else
                {
                    streetName.LegacyStreetNameId.Should().Be(new StreetNameId(snapshotStreetName.LegacyStreetNameId.Value));
                }

                streetName.LastProvenanceData.Should().BeEquivalentTo(snapshotStreetName.LastProvenanceData);
                streetName.LastEventHash.Should().Be(snapshotStreetName.LastEventHash);
            }
        }
    }
}
