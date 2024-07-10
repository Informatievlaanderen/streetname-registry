namespace StreetNameRegistry.Tests.AggregateTests.WhenRetiringStreetNamesForMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetNameRegistry.Municipality;
    using StreetNameRegistry.Municipality.Commands;
    using StreetNameRegistry.Municipality.Events;
    using StreetNameRegistry.Municipality.Exceptions;
    using StreetNameRegistry.Tests.AggregateTests.Extensions;
    using StreetNameRegistry.Tests.Builders;
    using StreetNameRegistry.Tests.Testing;
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
        public void ThenStreetNamesWereRetired()
        {
            var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>();

            var streetNamePersistentLocalIds = Fixture.CreateMany<PersistentLocalId>(5).ToList();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, new object[]
                    {
                        Fixture.Create<MunicipalityWasImported>(),
                        Fixture.Create<MunicipalityBecameCurrent>()
                    }
                    .Concat(streetNamePersistentLocalIds.SelectMany(persistentLocalId => new object[]
                    {
                        Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(persistentLocalId),
                        Fixture.Create<StreetNameWasApproved>().WithPersistentLocalId(persistentLocalId)
                    })
                    ).ToArray())
                .When(command)
                .Then(streetNamePersistentLocalIds
                    .Select(persistentLocalId => new Fact(
                        _streamId,
                        new StreetNameWasRetiredBecauseOfMunicipalityMerger(_municipalityId, new PersistentLocalId(persistentLocalId))
                    ))
                    .ToArray()));
        }

        [Fact]
        public void WithNoStreetName_ThenNone()
        {
            var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>();
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithRemovedStreetName_ThenNone()
        {
            var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var removedStreetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithIsRemoved()
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent,
                    removedStreetNameMigratedToMunicipality)
                .When(command)
                .ThenNone());
        }

        // [Fact]
        // public void WithRetiredMunicipality_ThenThrowsMunicipalityHasInvalidStatusException()
        // {
        //     var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>()
        //         .WithMunicipalityId(_municipalityId);
        //
        //     var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
        //     var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
        //         .WithStatus(StreetNameStatus.Proposed)
        //         .Build();
        //
        //     // Act, assert
        //     Assert(new Scenario()
        //         .Given(_streamId,
        //             municipalityWasImported,
        //             Fixture.Create<MunicipalityWasRetired>(),
        //             streetNameMigratedToMunicipality)
        //         .When(command)
        //         .Throws(new MunicipalityHasInvalidStatusException()));
        // }

        [Theory]
        [InlineData(StreetNameStatus.Proposed)]
        [InlineData(StreetNameStatus.Rejected)]
        public void WithInvalidStreetNameStatus_ThenNone(StreetNameStatus status)
        {
            var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>();

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
        public void WithRetiredStreetName_ThenNone()
        {
            var command = Fixture.Create<RetireStreetNamesForMunicipalityMerger>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Retired)
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
            Fixture.Customize(new WithFixedPersistentLocalId());

            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<StreetNameWasApproved>()
            });

            // Act
            aggregate.RetireStreetNamesForMunicipalityMerger();

            // Assert
            var result = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            result.Status.Should().Be(StreetNameStatus.Retired);
        }
    }
}
