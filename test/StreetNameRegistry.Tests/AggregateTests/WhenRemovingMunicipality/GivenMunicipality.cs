namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingMunicipality
{
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
        public void ThenMunicipalityWasRemoved()
        {
            var command = Fixture.Create<RemoveMunicipality>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, Assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Then(
                    new Fact(_streamId, new MunicipalityWasRemoved(
                        _municipalityId))
                ));
        }

        [Fact]
        public void WithExistingStreetName_ThenRemovesStreetName()
        {
            var persistentLocalId = 123456;
            Fixture.Register(() => new PersistentLocalId(persistentLocalId));
            Fixture.Register(() => new StreetName.PersistentLocalId(persistentLocalId));

            var command = Fixture.Create<RemoveMunicipality>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            Fixture.Register(() => false);
            var streetNameMigratedToMunicipality = Fixture.Create<StreetNameWasMigratedToMunicipality>();

            // Act, Assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported, streetNameMigratedToMunicipality)
                .When(command)
                .Then(
                    new Fact(_streamId, new StreetNameWasRemovedV2(_municipalityId, new PersistentLocalId(persistentLocalId))),
                    new Fact(_streamId, new MunicipalityWasRemoved(_municipalityId))
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            var persistentLocalId = 123456;
            Fixture.Register(() => new PersistentLocalId(persistentLocalId));
            Fixture.Register(() => new StreetName.PersistentLocalId(persistentLocalId));

            aggregate.Initialize(new List<object>
            {
                municipalityWasImported,
                Fixture.Create<StreetNameWasMigratedToMunicipality>(),
                Fixture.Create<StreetNameWasRemovedV2>(),
                Fixture.Create<MunicipalityWasRemoved>()
            });

            // Assert
            aggregate.StreetNames.Should().NotBeEmpty();
            aggregate.StreetNames.All(x => x.IsRemoved).Should().BeTrue();
            aggregate.IsRemoved.Should().BeTrue();
        }
    }
}
