namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingStreetNameRejection
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Extensions;
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
            Fixture.Customize(new WithFixedPersistentLocalId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNameRejectionWasCorrectedProposed()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasRejected>())
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasCorrectedFromRejectedToProposed(_municipalityId, command.PersistentLocalId))));
        }

        [Fact]
        public void WithRetiredMunicipality_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Rejected)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityWasRetired>(),
                    streetNameWasMigrated)
                .When(command)
                .Throws(new MunicipalityHasInvalidStatusException()));
        }

        [Fact]
        public void WithoutProposedStreetName_ThenThrowsStreetNameIsNotFoundException()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithRemovedStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Rejected)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigrated)
                .When(command)
                .Throws(new StreetNameIsRemovedException(command.PersistentLocalId)));
        }

        [Theory]
        [InlineData(StreetNameStatus.Current)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithInvalidStreetNameStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(status)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigrated)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithActiveExistingStreetName_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });

            var command = Fixture.Create<CorrectStreetNameRejection>()
                .WithPersistentLocalId(new PersistentLocalId(1));

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(new PersistentLocalId(1)),
                    Fixture.Create<StreetNameWasRejected>().WithPersistentLocalId(new PersistentLocalId(1)),
                    Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(new PersistentLocalId(2)))
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void WithAlreadyProposedStreetName_ThenNone()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Proposed)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var municipality = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            municipality.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<StreetNameWasRejected>()
            });

            // Act
            municipality.CorrectStreetNameRejection(persistentLocalId);

            // Assert
            var result = municipality.StreetNames.GetByPersistentLocalId(persistentLocalId);
            result.Status.Should().Be(StreetNameStatus.Proposed);
        }
    }
}
