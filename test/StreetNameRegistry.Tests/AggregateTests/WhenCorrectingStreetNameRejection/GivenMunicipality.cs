namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingStreetNameRejection
{
    using System.Collections.Generic;
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
        public void OnRetiredMunicipality_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        Fixture.Create<PersistentLocalId>(),
                        StreetNameStatus.Rejected,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        new HomonymAdditions(),
                        isCompleted: true,
                        isRemoved: false);
                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

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
        public void OnRemovedStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        Fixture.Create<PersistentLocalId>(),
                        StreetNameStatus.Rejected,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        new HomonymAdditions(),
                        isCompleted: true,
                        isRemoved: true);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

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
        public void OnStreetNameWithInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        Fixture.Create<PersistentLocalId>(),
                        status,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        new HomonymAdditions(),
                        isCompleted: true,
                        isRemoved: false);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

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
        public void WithActiveExistingStreetName_ThenStreetNameNameAlreadyExistsExceptionWasThrown()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });

            var command = Fixture.Create<CorrectStreetNameRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasRejected>(),
                    Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(new PersistentLocalId(1)))
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void WithStreetNameAlreadyProposed_ThenNone()
        {
            var command = Fixture.Create<CorrectStreetNameRejection>();

            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        Fixture.Create<PersistentLocalId>(),
                        StreetNameStatus.Proposed,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        new HomonymAdditions(),
                        isCompleted: true,
                        isRemoved: false);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

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
