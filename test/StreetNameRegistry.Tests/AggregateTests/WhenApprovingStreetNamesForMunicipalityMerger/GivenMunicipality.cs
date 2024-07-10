namespace StreetNameRegistry.Tests.AggregateTests.WhenApprovingStreetNamesForMunicipalityMerger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNamesWereApproved()
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();
            var streetNameWasProposedList = Fixture.CreateMany<StreetNameWasProposedV2>(5).ToList();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, new object[] {
                        Fixture.Create<MunicipalityWasImported>(),
                        Fixture.Create<MunicipalityBecameCurrent>()
                    }
                    .Concat(streetNameWasProposedList)
                    .ToArray()
                )
                .When(command)
                .Then(streetNameWasProposedList
                    .Select(streetNameWasProposed => new Fact(
                        _streamId,
                        new StreetNameWasApproved(_municipalityId, new PersistentLocalId(streetNameWasProposed.PersistentLocalId))
                    ))
                    .ToArray()));
        }

        [Fact]
        public void WithNoStreetName_ThenNone()
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithRemovedStreetName_ThenNone()
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent,
                    streetNameMigratedToMunicipality)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithInvalidMunicipalityStatus_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
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
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithInvalidStreetNameStatus_ThenNone(StreetNameStatus status)
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(status)
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
        public void WithStreetNameAlreadyCurrent_ThenNone()
        {
            var command = Fixture.Create<ApproveStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
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
        public void StateCheck()
        {
            var persistentLocalIds = Fixture.CreateMany<PersistentLocalId>(5).ToArray();
            persistentLocalIds.Should().NotBeEmpty();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>()
            }.Concat(persistentLocalIds.Select(persistentLocalId => Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(persistentLocalId))));

            // Act
            aggregate.ApproveStreetNamesForMunicipalityMerger();

            // Assert
            foreach (var persistentLocalId in persistentLocalIds)
            {
                var result = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
                result.Status.Should().Be(StreetNameStatus.Current);
            }
        }
    }
}
