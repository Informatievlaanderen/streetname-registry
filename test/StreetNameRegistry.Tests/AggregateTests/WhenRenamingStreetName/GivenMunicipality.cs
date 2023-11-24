namespace StreetNameRegistry.Tests.AggregateTests.WhenRenamingStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customizations.Add(new WithUniqueInteger());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenSourceStreetNameWasRenamedAndDestinationStreetNameWasApproved()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .Build();

            var sourceStreetNameWasApproved = new StreetNameWasApprovedBuilder(Fixture)
                .WithPersistentLocalId(sourceStreetNameWasProposed.PersistentLocalId)
                .Build();

            var destinationStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameWasProposed,
                    sourceStreetNameWasApproved,
                    destinationStreetNameWasProposed)
                .When(command)
                .Then(
                    new Fact(_streamId, new StreetNameWasApproved(_municipalityId, command.DestinationPersistentLocalId)),
                    new Fact(_streamId, new StreetNameWasRenamed(_municipalityId, command.PersistentLocalId, command.DestinationPersistentLocalId))));
        }

        [Fact]
        public void WithApprovedDestinationStreetName_ThenSourceStreetNameWasRenamed()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .Build();

            var sourceStreetNameWasApproved = new StreetNameWasApprovedBuilder(Fixture)
                .WithPersistentLocalId(sourceStreetNameWasProposed.PersistentLocalId)
                .Build();

            var destinationStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            var destinationStreetNameWasApproved = new StreetNameWasApprovedBuilder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameWasProposed,
                    sourceStreetNameWasApproved,
                    destinationStreetNameWasProposed,
                    destinationStreetNameWasApproved)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasRenamed(_municipalityId, command.PersistentLocalId, command.DestinationPersistentLocalId))));
        }

        [Fact]
        public void WithUnexistingSourceStreetName_ThenThrowsStreetNameIsNotFoundException()
        {
            var command = Fixture.Create<RenameStreetName>();

            var destinationStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    destinationStreetNameWasProposed)
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithUnexistingDestinationStreetName_ThenThrowsStreetNameIsNotFoundException()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .Build();

            var sourceStreetNameWasApproved = new StreetNameWasApprovedBuilder(Fixture)
                .WithPersistentLocalId(sourceStreetNameWasProposed.PersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameWasProposed,
                    sourceStreetNameWasApproved)
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithRemovedSourceStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .WithStatus(StreetNameStatus.Current)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameMigrated)
                .When(command)
                .Throws(new StreetNameIsRemovedException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithRemovedDestinationStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .WithStatus(StreetNameStatus.Current)
                .Build();

            var destinationStreetNameMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameMigrated,
                    destinationStreetNameMigrated)
                .When(command)
                .Throws(new StreetNameIsRemovedException(command.DestinationPersistentLocalId)));
        }

        [Fact]
        public void WithInvalidMunicipalityStatus_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .Build();

            var destinationStreetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityWasRetired>(),
                    sourceStreetNameWasProposed,
                    destinationStreetNameWasProposed)
                .When(command)
                .Throws(new MunicipalityHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Proposed)]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithInvalidSourceStreetNameStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .WithStatus(status)
                .Build();

            var destinationStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameMigratedToMunicipality,
                    destinationStreetNameMigratedToMunicipality)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithInvalidDestinationStreetNameStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<RenameStreetName>();

            var sourceStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.PersistentLocalId)
                .WithStatus(StreetNameStatus.Current)
                .Build();

            var destinationStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(command.DestinationPersistentLocalId)
                .WithStatus(status)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    sourceStreetNameMigratedToMunicipality,
                    destinationStreetNameMigratedToMunicipality)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var persistentLocalId = new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId);

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                streetNameWasProposedV2,
                new StreetNameWasRenamedBuilder(Fixture)
                    .WithPersistentLocalId(persistentLocalId)
                    .Build()
            });

            var result = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            result.Status.Should().Be(StreetNameStatus.Retired);
        }
    }
}
