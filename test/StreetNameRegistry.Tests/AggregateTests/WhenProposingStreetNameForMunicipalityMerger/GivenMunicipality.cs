namespace StreetNameRegistry.Tests.AggregateTests.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Extensions;
    using Builders;
    using FluentAssertions;
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
        public void ThenStreetNameWasProposedForMunicipalityMerger()
        {
            //Arrange
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames([new(Fixture.Create<string>(), Language.Dutch)])
                .WithHomonymAdditions([new (new string(Fixture.CreateMany<char>(5).ToArray()), Language.Dutch)])
                ;

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();

            //Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Theory]
        [InlineData("Bremstraat", "Bremstraat")]
        [InlineData("Bremstraat", "bremstraat")]
        public void WithExistingStreetNameName_ThenThrowsStreetNameNameAlreadyExistsException(string name, string newName)
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>()
                .WithNames(new Names(new List<StreetNameName> { new(name, Language.Dutch) }));

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(new Names(new List<StreetNameName> { new(newName, Language.Dutch) }));

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(newName)));
        }

        [Fact]
        public void WithExistingPersistentLocalId_ThenThrowsStreetNamePersistentLocalIdAlreadyExistsException()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithPersistentLocalId(new PersistentLocalId(streetNameWasProposed.PersistentLocalId));

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    streetNameWasProposed)
                .When(command)
                .Throws(new StreetNamePersistentLocalIdAlreadyExistsException()));
        }

        [Fact]
        public void WithExistingRetiredStreetName_ThenStreetNameWasProposed()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => StreetNameStatus.Retired);
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = Fixture.Create<StreetNameWasMigratedToMunicipality>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Fact]
        public void WithExistingRejectedStreetName_ThenStreetNameWasProposed()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => StreetNameStatus.Rejected);
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = Fixture.Create<StreetNameWasMigratedToMunicipality>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Fact]
        public void WithExistingRemovedStreetName_ThenStreetNameWasProposed()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithIsRemoved()
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Fact]
        public void WithOneExistingStreetNameAndOneNew_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var existingStreetNameName = Fixture.Create<StreetNameName>();
            var newStreetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { existingStreetNameName });

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(new Names { existingStreetNameName, newStreetNameName });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    streetNameWasProposed)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(existingStreetNameName.Name)));
        }

        [Fact]
        public void WithNoConflictingStreetNames_ThenStreetNameWasProposed()
        {
            Fixture.Register(() => Taal.NL);
            Fixture.Register(() => Language.Dutch);

            var existingStreetNameName = Fixture.Create<StreetNameName>();
            var newStreetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { existingStreetNameName });

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(new Names { newStreetNameName });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Fact]
        public void WithMunicipalityRetired_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityWasRetired = Fixture.Create<MunicipalityWasRetired>();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithRandomStreetName(Fixture);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityWasRetired)
                .When(command)
                .Throws(new MunicipalityHasInvalidStatusException($"Municipality with id '{_municipalityId}' was retired")));
        }

        [Fact]
        public void WithOfficialLanguageDutchAndProposedLanguageIsFrench_ThenThrowsStreetNameNameLanguageNotSupportedException()
        {
            Fixture.Register(() => Language.Dutch);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();

            var names = new Names
            {
                new(Fixture.Create<string>(), Language.French)
            };

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(names);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded)
                .When(command)
                .Throws(new StreetNameNameLanguageIsNotSupportedException(
                    $"The language '{Language.French}' is not an official or facility language of municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void WithFacilityLanguageFrenchAndProposedLanguageIsDutch_ThenThrowsStreetNameNameLanguageNotSupportedException()
        {
            Fixture.Register(() => Language.French);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityFacilityLanguageWasAdded = Fixture.Create<MunicipalityFacilityLanguageWasAdded>();

            var names = new Names
            {
                new(Fixture.Create<string>(), Language.Dutch)
            };

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(names);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityFacilityLanguageWasAdded)
                .When(command)
                .Throws(new StreetNameNameLanguageIsNotSupportedException(
                    $"The language '{Language.Dutch}' is not an official or facility language of municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void
            WithOfficialLanguageDutchAndFacilityLanguageFrenchAndProposedLanguageIsFrench_ThenThrowsStreetNameNameLanguageNotSupportedException()
        {
            Fixture.Register(() => Language.Dutch);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityDutchOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            Fixture.Register(() => Language.French);
            var municipalityFacilityLanguageWasAdded = Fixture.Create<MunicipalityFacilityLanguageWasAdded>();

            var names = new Names
            {
                new(Fixture.Create<string>(), Language.French)
            };

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithStreetNameNames(names);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityDutchOfficialLanguageWasAdded,
                    municipalityFacilityLanguageWasAdded)
                .When(command)
                .Throws(new StreetNameIsMissingALanguageException($"The language '{Language.Dutch}' is missing.")));
        }

        [Fact]
        public void WithExistingStreetNameAndNoHomonymAdditions_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void WithExistingStreetNameAndExistingHomonymAdditions_ThenStreetNameWasProposed()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithHomonymAdditions(new HomonymAdditions() { new("test", Language.Dutch) })
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithHomonymAdditions(new HomonymAdditions() { new("test", Language.Dutch) });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void WithExistingStreetNameAndHomonymAdditions_ThenStreetNameWasProposed()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithHomonymAdditions(new HomonymAdditions() { new("test", Language.Dutch) })
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedForMunicipalityMerger(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.HomonymAdditions,
                        command.PersistentLocalId,
                        command.MergedStreetNamePersistentLocalIds))));
        }

        [Fact]
        public void WithEmptyMergedStreetNamePersistentLocalIds_ThenThrowsMergedStreetNamePersistentLocalIdsAreMissingException()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithRandomStreetName(Fixture)
                .WithMergedStreetNamePersistentIds(new List<PersistentLocalId>());

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Throws(new MergedStreetNamePersistentLocalIdsAreMissingException()));
        }

        [Fact]
        public void WithDuplicateMergedStreetNamePersistentLocalIds_ThenThrowsMergedStreetNamePersistentLocalIdsAreNotUniqueException()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .Build();

            var command = Fixture.Create<ProposeStreetNameForMunicipalityMerger>()
                .WithRandomStreetName(Fixture)
                .WithMergedStreetNamePersistentIds(new List<PersistentLocalId> { new(5), new(5) });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Throws(new MergedStreetNamePersistentLocalIdsAreNotUniqueException()));
        }

        [Fact]
        public void StateCheck()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            var homonym = new StreetNameHomonymAddition(new string(Fixture.CreateMany<char>(5).ToArray()), Language.Dutch);
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => new HomonymAdditions { homonym });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var streetNameWasProposedForMunicipalityMerger = Fixture.Create<StreetNameWasProposedForMunicipalityMerger>();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                streetNameWasProposedForMunicipalityMerger
            });

            // Assert
            var result = aggregate.StreetNames.GetByPersistentLocalId(new PersistentLocalId(streetNameWasProposedForMunicipalityMerger.PersistentLocalId));
            result.Status.Should().Be(StreetNameStatus.Proposed);
            result.Names.Count.Should().BeGreaterThan(0);
            result.Names.Should().BeEquivalentTo(new Names(streetNameWasProposedForMunicipalityMerger.StreetNameNames));
            result.HomonymAdditions.Should().BeEquivalentTo(new HomonymAdditions(streetNameWasProposedForMunicipalityMerger.HomonymAdditions));
            result.MergedStreetNamePersistentLocalIds.Should().BeEquivalentTo(streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)));
        }
    }
}
