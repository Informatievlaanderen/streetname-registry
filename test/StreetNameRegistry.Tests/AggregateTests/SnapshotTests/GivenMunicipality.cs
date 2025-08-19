namespace StreetNameRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Newtonsoft.Json;
    using Extensions;
    using NodaTime;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _streamId = new MunicipalityStreamId(Fixture.Create<MunicipalityId>());
        }

        [Fact]
        public async Task ThenSnapshotWasCreated()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var provenance = Fixture.Create<Provenance>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var officialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.Dutch).ToEvent();
            var officialLanguageWasAdded2 = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.French).ToEvent();
            var facilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.English).ToEvent();

            var existingStreetNameWasProposed = new StreetNameWasProposedV2(
                Fixture.Create<MunicipalityId>(),
                new NisCode(municipalityWasImported.NisCode),
                new Names(Fixture.Create<Dictionary<Language, string>>()),
                Fixture.Create<PersistentLocalId>());
            ((ISetProvenance)existingStreetNameWasProposed).SetProvenance(provenance);

            var proposeNewStreetName = new ProposeStreetName(
                Fixture.Create<MunicipalityId>(),
                new Names(new List<StreetNameName>
                {
                    new StreetNameName(Fixture.Create<string>(), Language.Dutch),
                    new StreetNameName(Fixture.Create<string>(), Language.French),
                    new StreetNameName(Fixture.Create<string>(), Language.English)
                }),
                Fixture.Create<PersistentLocalId>(),
                provenance);

            var newStreetNameWasProposed = new StreetNameWasProposedV2(
                proposeNewStreetName.MunicipalityId,
                new NisCode(municipalityWasImported.NisCode),
                proposeNewStreetName.StreetNameNames,
                proposeNewStreetName.PersistentLocalId);
            ((ISetProvenance)newStreetNameWasProposed).SetProvenance(provenance);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent,
                    officialLanguageWasAdded,
                    officialLanguageWasAdded2,
                    facilityLanguageWasAdded,
                    existingStreetNameWasProposed)
                .When(proposeNewStreetName)
                .Then(new Fact(_streamId,
                    newStreetNameWasProposed)));

            var existingStreetName = new MunicipalityStreetName(o => { });
            existingStreetName.Route(existingStreetNameWasProposed);
            var newStreetName = new MunicipalityStreetName(o => { });
            newStreetName.Route(newStreetNameWasProposed);

            var expectedSnapshot = new MunicipalitySnapshot(
                Fixture.Create<MunicipalityId>(),
                new NisCode(municipalityWasImported.NisCode),
                MunicipalityStatus.Current,
                new List<Language> { Language.Dutch, Language.French },
                new List<Language> { Language.English },
                new MunicipalityStreetNames
                {
                    existingStreetName,
                    newStreetName
                },
                false);

            var snapshotStore = (ISnapshotStore)Container.Resolve(typeof(ISnapshotStore));
            var latestSnapshot = await snapshotStore.FindLatestSnapshotAsync(_streamId, CancellationToken.None);

            latestSnapshot.Should().NotBeNull();
            var snapshot = JsonConvert.DeserializeObject<MunicipalitySnapshot>(latestSnapshot!.Data, EventSerializerSettings);

            snapshot.Should().BeEquivalentTo(expectedSnapshot, options =>
            {
                options.Excluding(x => x.Path.EndsWith("LastEventHash"));
                options.Excluding(x => x.Type == typeof(Instant));
                return options;
            });
        }

        private static SnapshotContainer Build(
            MunicipalitySnapshot snapshot,
            long streamVersion,
            JsonSerializerSettings serializerSettings)
        {
            return new SnapshotContainer
            {
                Info = new SnapshotInfo { StreamVersion = streamVersion, Type = nameof(MunicipalitySnapshot) },
                Data = JsonConvert.SerializeObject(snapshot, serializerSettings)
            };
        }
    }
}
