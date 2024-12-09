namespace StreetNameRegistry.Tests.AggregateTests.WhenRetiringMunicipalityForMunicipalityMerger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using Extensions;
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

        public GivenMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void WithCurrentStreetNames_ThenStreetNamesWereRetired()
        {
            var command = Fixture.Create<RetireMunicipalityForMunicipalityMerger>();

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
                    )
                    .ToArray())
                .When(command)
                .Then(Array.Empty<Fact>()
                    .Concat(streetNamePersistentLocalIds
                        .Select(persistentLocalId => new Fact(
                            _streamId,
                            new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                                _municipalityId,
                                new PersistentLocalId(persistentLocalId),
                                [])
                        )))
                    .Concat([new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))])
                    .ToArray()));
        }

        [Fact]
        public void WithRemovedNewStreetName_ThenNotInMergedPersistentLocalIds()
        {
            Fixture.Customizations.Add(new WithUniqueInteger());

            var oldStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var oldMunicipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var oldStreetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>()
                .WithPersistentLocalId(oldStreetNamePersistentLocalId);
            var oldStreetNameWasApproved = Fixture.Create<StreetNameWasApproved>()
                .WithPersistentLocalId(oldStreetNamePersistentLocalId);

            var newMunicipalityId = new MunicipalityId(Guid.NewGuid());
            var newMunicipalityStreamId = new MunicipalityStreamId(newMunicipalityId);
            var newMunicipalityWasImported = Fixture.Create<MunicipalityWasImported>()
                .WithMunicipalityId(newMunicipalityId);
            var newStreetNameWasProposedForMunicipalityMerger = Fixture.Create<StreetNameWasProposedForMunicipalityMerger>()
                .WithMunicipalityId(newMunicipalityId)
                .WithMergedPersistentLocalIds(oldStreetNamePersistentLocalId);
            var newStreetNameWasProposedForMunicipalityMergerToRemove = Fixture.Create<StreetNameWasProposedForMunicipalityMerger>()
                .WithMunicipalityId(newMunicipalityId)
                .WithMergedPersistentLocalIds(oldStreetNamePersistentLocalId);
            var streetNameWasRemovedV2 = Fixture.Create<StreetNameWasRemovedV2>()
                .WithMunicipalityId(newMunicipalityId)
                .WithPersistentLocalId(new PersistentLocalId(newStreetNameWasProposedForMunicipalityMergerToRemove.PersistentLocalId));

            var command = new RetireMunicipalityForMunicipalityMerger(_municipalityId, newMunicipalityId, Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(
                    new Fact(_streamId, oldMunicipalityWasImported),
                    new Fact(_streamId, Fixture.Create<MunicipalityBecameCurrent>()),
                    new Fact(_streamId, oldStreetNameWasProposedV2),
                    new Fact(_streamId, oldStreetNameWasApproved),

                    new Fact(newMunicipalityStreamId, newMunicipalityWasImported),
                    new Fact(newMunicipalityStreamId, newStreetNameWasProposedForMunicipalityMerger),
                    new Fact(newMunicipalityStreamId, newStreetNameWasProposedForMunicipalityMergerToRemove),
                    new Fact(newMunicipalityStreamId, streetNameWasRemovedV2)
                    )
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                            _municipalityId,
                            new PersistentLocalId(oldStreetNamePersistentLocalId),
                            [new PersistentLocalId(newStreetNameWasProposedForMunicipalityMerger.PersistentLocalId)])),
                    new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))
                    ));
        }

        [Fact]
        public void WithProposedStreetNames_ThenStreetNamesWereRejected()
        {
            var command = Fixture.Create<RetireMunicipalityForMunicipalityMerger>();

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
                            Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(persistentLocalId)
                        })
                    )
                    .ToArray())
                .When(command)
                .Then(Array.Empty<Fact>()
                    .Concat(streetNamePersistentLocalIds
                        .Select(persistentLocalId => new Fact(
                            _streamId,
                            new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                                _municipalityId,
                                new PersistentLocalId(persistentLocalId),
                                [])
                        )))
                    .Concat([new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))])
                    .ToArray()));
        }

        [Fact]
        public void WithNoStreetNames_ThenMunicipalityWasMerged()
        {
            var command = Fixture.Create<RetireMunicipalityForMunicipalityMerger>();
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))));
        }

        [Fact]
        public void WithRemovedStreetNames_ThenMunicipalityWasMerged()
        {
            var command = Fixture.Create<RetireMunicipalityForMunicipalityMerger>();

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
                .Then(new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))));
        }

        [Theory]
        [InlineData(StreetNameStatus.Retired)]
        [InlineData(StreetNameStatus.Rejected)]
        public void WithInvalidStreetNameStatus_ThenMunicipalityWasMerged(StreetNameStatus status)
        {
            var command = Fixture.Create<RetireMunicipalityForMunicipalityMerger>();

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
                .Then(new Fact(_streamId, new MunicipalityWasMerged(_municipalityId, command.NewMunicipalityId))));
        }

        [Fact]
        public void StateCheck()
        {
            var currentPersistentLocalId = Fixture.Create<PersistentLocalId>();
            var proposedPersistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(currentPersistentLocalId),
                Fixture.Create<StreetNameWasApproved>().WithPersistentLocalId(currentPersistentLocalId),
                Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(proposedPersistentLocalId)
            });

            var newMunicipalityId = new MunicipalityId(Guid.NewGuid());

            var newMunicipality = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            newMunicipality.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>().WithMunicipalityId(newMunicipalityId),
                Fixture.Create<MunicipalityBecameCurrent>().WithMunicipalityId(newMunicipalityId)
            });

            // Act
            aggregate.RetireForMunicipalityMerger(newMunicipality);

            // Assert
            aggregate.MunicipalityStatus.Should().Be(MunicipalityStatus.Retired);

            var retiredStreetName = aggregate.StreetNames.GetByPersistentLocalId(currentPersistentLocalId);
            retiredStreetName.Status.Should().Be(StreetNameStatus.Retired);

            var rejectedStreetName = aggregate.StreetNames.GetByPersistentLocalId(proposedPersistentLocalId);
            rejectedStreetName.Status.Should().Be(StreetNameStatus.Rejected);
        }
    }
}
