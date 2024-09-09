namespace StreetNameRegistry.Tests.AggregateTests.WhenMigratingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNameWasMigrated()
        {
            var command = Fixture.Create<MigrateStreetNameToMunicipality>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, Assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Then(
                    new Fact(_streamId, new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameId,
                        command.PersistentLocalId,
                        command.Status,
                        command.PrimaryLanguage,
                        command.SecondaryLanguage,
                        command.Names,
                        command.HomonymAdditions,
                        command.IsCompleted,
                        command.IsRemoved))
                ));
        }

        [Fact]
        public void WithExistingStreetNamePersistentLocalId_ThenThrowsInvalidOperationException()
        {
            var persistentLocalId = 123456;
            Fixture.Register(() => new PersistentLocalId(persistentLocalId));
            Fixture.Register(() => new StreetName.PersistentLocalId(persistentLocalId));

            var command = Fixture.Create<MigrateStreetNameToMunicipality>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = Fixture.Create<StreetNameWasMigratedToMunicipality>();

            // Act, Assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported, streetNameMigratedToMunicipality)
                .When(command)
                .Throws(new InvalidOperationException($"Cannot migrate StreetName with id '{persistentLocalId}' in municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            aggregate.Initialize(new List<object>
            {
                municipalityWasImported
            });

            // Act
            var migratedStreetName = Fixture.Create<StreetNameWasMigratedToMunicipality>();

            aggregate.MigrateStreetName(
                new StreetNameId(migratedStreetName.StreetNameId),
                new PersistentLocalId(migratedStreetName.PersistentLocalId),
                migratedStreetName.Status,
                migratedStreetName.PrimaryLanguage,
                migratedStreetName.SecondaryLanguage,
                new Names(migratedStreetName.Names),
                new HomonymAdditions(migratedStreetName.HomonymAdditions),
                migratedStreetName.IsCompleted,
                migratedStreetName.IsRemoved);

            // Assert
            aggregate.StreetNames.Should().NotBeEmpty();

            var streetName = aggregate.StreetNames.First();
            streetName.LegacyStreetNameId.Should().Be(new StreetNameId(migratedStreetName.StreetNameId));
            streetName.PersistentLocalId.Should().Be(new PersistentLocalId(migratedStreetName.PersistentLocalId));
            streetName.Status.Should().Be(migratedStreetName.Status);
            streetName.Names.Should().BeEquivalentTo(migratedStreetName.Names);
            streetName.HomonymAdditions.Should().BeEquivalentTo(migratedStreetName.HomonymAdditions);
            streetName.IsRemoved.Should().Be(migratedStreetName.IsRemoved);
        }
    }
}
