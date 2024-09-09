namespace StreetNameRegistry.Tests.AggregateTests.WhenProposingStreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
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

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNameWasProposed()
        {
            //Arrange
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
                .WithRandomStreetName(Fixture);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();

            //Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported, municipalityOfficialLanguageWasAdded)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames,
                        command.PersistentLocalId))));
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames,
                        command.PersistentLocalId))));
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

            var command = Fixture.Create<ProposeStreetName>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames,
                        command.PersistentLocalId))));
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

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames,
                        command.PersistentLocalId))));
        }

        [Fact]
        public void WithOneExistingStreetNameAndOneNew_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var existingStreetNameName = Fixture.Create<StreetNameName>();
            var newStreetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { existingStreetNameName });

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
                .WithStreetNameNames(new Names { newStreetNameName });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(
                        _municipalityId,
                        new NisCode(municipalityWasImported.NisCode),
                        command.StreetNameNames,
                        command.PersistentLocalId))));
        }

        [Fact]
        public void WithMunicipalityRetired_ThenThrowsMunicipalityHasInvalidStatusException()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityWasRetired = Fixture.Create<MunicipalityWasRetired>();

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>()
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

            var command = Fixture.Create<ProposeStreetName>();

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames,
                        command.PersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            var names = Fixture.Create<Names>();
            var persistentLocalId = Fixture.Create<PersistentLocalId>();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>()
            });

            // Act
            aggregate.ProposeStreetName(names, persistentLocalId);

            // Assert
            aggregate.StreetNames.Should().NotBeEmpty();

            var streetName = aggregate.StreetNames.First();

            streetName.PersistentLocalId.Should().Be(persistentLocalId);
            streetName.Names.Should().BeEquivalentTo(names);
        }
    }
}
