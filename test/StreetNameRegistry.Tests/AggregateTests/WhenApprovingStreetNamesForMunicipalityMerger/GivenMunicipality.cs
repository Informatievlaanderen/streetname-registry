namespace StreetNameRegistry.Tests.AggregateTests.WhenApprovingStreetNamesForMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Linq;
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
            var streetNameWasProposedList = new[]
            {
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(null)
                    .Build(),
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(StreetNameStatus.Proposed)
                    .Build(),
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(StreetNameStatus.Current)
                    .Build(),
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(StreetNameStatus.Current)
                    .Build(),
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(StreetNameStatus.Rejected)
                    .Build(),
                new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                    .WithDesiredStatus(StreetNameStatus.Retired)
                    .Build(),
            };

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, new object[]
                    {
                        Fixture.Create<MunicipalityWasImported>(),
                        Fixture.Create<MunicipalityBecameCurrent>()
                    }
                    .Concat(streetNameWasProposedList)
                    .ToArray()
                )
                .When(command)
                .Then(streetNameWasProposedList.Where(x => x.DesiredStatus == StreetNameStatus.Current)
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
            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                .WithDesiredStatus(StreetNameStatus.Current)
                .Build();
            var streetNameWasRemovedV2 = new StreetNameWasRemovedV2(
                Fixture.Create<MunicipalityId>(),
                new PersistentLocalId(streetNameWasProposedForMunicipalityMerger.PersistentLocalId));
            ((ISetProvenance)streetNameWasRemovedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent,
                    streetNameWasProposedForMunicipalityMerger,
                    streetNameWasRemovedV2)
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
            var streetNameToRemainProposedPersistentLocalId = new PersistentLocalId(persistentLocalIds.Sum(x => (int)x));

            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            aggregate.Initialize(
                new List<object>
                {
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                        .WithPersistentLocalId(streetNameToRemainProposedPersistentLocalId)
                        .WithDesiredStatus(StreetNameStatus.Proposed)
                        .Build()
                }.Concat(persistentLocalIds.Select(x =>
                    new StreetNameWasProposedForMunicipalityMergerBuilder(Fixture)
                        .WithPersistentLocalId(x)
                        .WithDesiredStatus(StreetNameStatus.Current)
                        .Build())));

            aggregate.ApproveStreetNamesForMunicipalityMerger();

            foreach (var persistentLocalId in persistentLocalIds)
            {
                var result = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
                result.Status.Should().Be(StreetNameStatus.Current);
            }

            aggregate.StreetNames
                .GetByPersistentLocalId(streetNameToRemainProposedPersistentLocalId)
                .Status
                .Should()
                .Be(StreetNameStatus.Proposed);
        }
    }
}
