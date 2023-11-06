namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingRetirementStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Extensions;
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
            Fixture.Customize(new WithFixedPersistentLocalId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNameRetirementWasCorrected()
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasApproved>(),
                    Fixture.Create<StreetNameWasRetiredV2>())
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasCorrectedFromRetiredToCurrent(_municipalityId, command.PersistentLocalId))));
        }

        [Fact]
        public void WithExistingStreetName_ThenStreetNameNameAlreadyExistsExceptionWasThrown()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => new PersistentLocalId(2));

            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasRetiredV2>(),
                    Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(new PersistentLocalId(1)))
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void ThenStreetNameNotFoundExceptionWasThrown()
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void ThenStreetNameIsRemovedExceptionWasThrown()
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var removedStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent,
                    removedStreetNameMigratedToMunicipality)
                .When(command)
                .Throws(new StreetNameIsRemovedException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithMunicipalityStatusRetired_ThenMunicipalityHasInvalidStatusExceptionWasThrown()
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithStatus(StreetNameStatus.Proposed)
                .WithPrimaryLanguage(Language.Dutch)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    Fixture.Create<MunicipalityWasRetired>(),
                    streetNameMigratedToMunicipality)
                .When(command)
                .Throws(new MunicipalityHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Proposed)]
        [InlineData(StreetNameStatus.Rejected)]
        public void ThenStreetNameHasInvalidStatusExceptionWasThrown(StreetNameStatus status)
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithStatus(status)
                .WithPrimaryLanguage(Language.Dutch)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameMigratedToMunicipality)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithCurrentStreetName_ThenNone()
        {
            var command = Fixture.Create<CorrectStreetNameRetirement>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameMigratedToMunicipality)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void ThenStreetNameStatusIsRetired()
        {
            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<StreetNameWasApproved>(),
                Fixture.Create<StreetNameWasRetiredV2>()
            });

            // Act
            aggregate.CorrectStreetNameRetirement(persistentLocalId);

            // Assert
            var result = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            result.Status.Should().Be(StreetNameStatus.Current);
        }
    }
}
