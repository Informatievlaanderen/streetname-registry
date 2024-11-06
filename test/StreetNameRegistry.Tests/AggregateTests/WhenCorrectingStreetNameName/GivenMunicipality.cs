namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingStreetNameName
{
    using System.Collections.Generic;
    using System.Linq;
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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Builders;

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

            Fixture.Register(() => new Names(Fixture.Create<Dictionary<Language, string>>()));
        }

        [Fact]
        public void ThenStreetNameNameWasCorrected()
        {
            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposed = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("1011"),
                new Names(new List<StreetNameName>
                {
                    new("Kaplestraat", Language.Dutch),
                    new("Rue d'la Croix - Rouge", Language.French),
                }),
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposed.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(new Names())
                .WithStreetNameName(new StreetNameName("Kapelstraat", Language.Dutch))
                .WithStreetNameName(new StreetNameName("Rue de la Croix - Rouge", Language.French));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(_municipalityId,
                    command.PersistentLocalId, command.StreetNameNames))
                ));
        }

        [Fact]
        public void WithCapitalizationChange_ThenStreetNameNameWasCorrected()
        {
            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var streetNameName = "Kapelstraat";

            var streetNameWasProposed = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("1011"),
                new Names(new List<StreetNameName>
                {
                    new(streetNameName.ToLower(), Language.Dutch),
                }),
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposed.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(new Names())
                .WithStreetNameName(new StreetNameName(streetNameName, Language.Dutch));
            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(_municipalityId,
                    command.PersistentLocalId, command.StreetNameNames))
                ));
        }

        [Fact]
        public void WithACorrectionThatExceedsLevenshteinThreshold_ThenThrowsStreetNameNameCorrectionExceededCharacterChangeLimitException()
        {
            var originalName = "0123546789";
            var correctedName = "012354----";

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode(new NisCode("1011"))
                .WithNames(new Names(new List<StreetNameName>
                {
                    new(originalName, Language.Dutch)
                }))
                .Build();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(new Names())
                .WithStreetNameName(new StreetNameName(correctedName, Language.Dutch));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Throws(new StreetNameNameCorrectionExceededCharacterChangeLimitException(correctedName)));
        }

        [Fact]
        public void WithACorrectionExactly30Percent_ThenStreetNameWasCorrected()
        {
            var originalName = "0123456789";
            var correctedName = "0123456---";

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposed = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode(new NisCode("1011"))
                .WithNames(new Names(new List<StreetNameName>
                {
                    new(originalName, Language.Dutch)
                }))
                .Build();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(new Names())
                .WithStreetNameName(new StreetNameName(correctedName, Language.Dutch));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(_municipalityId,
                    command.PersistentLocalId, command.StreetNameNames))
                ));
        }

        [Fact]
        public void WithStreetNameNotFound_ThenThrowsStreetNameNotFoundException()
        {
            var command = Fixture.Create<CorrectStreetNameNames>();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithRemovedStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<CorrectStreetNameNames>();

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
                .Throws(new StreetNameIsRemovedException(command.PersistentLocalId)));
        }

        [Theory]
        [InlineData(StreetNameStatus.Retired)]
        [InlineData(StreetNameStatus.Rejected)]
        public void WithInvalidStreetNameStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<CorrectStreetNameNames>();

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
                .Throws(new StreetNameHasInvalidStatusException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithDuplicateNameAndRejectedStatus_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNameNames = new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) });
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode(new NisCode("abc"))
                .WithNames(streetNameNames)
                .WithPersistentLocalId(new PersistentLocalId(command.PersistentLocalId + 1))
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    Fixture.Create<StreetNameWasProposedV2>(), //123
                    streetNameWasProposedV2, //123
                    Fixture.Create<StreetNameWasRejected>())
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithStreetNameNameAlreadyInUse_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var streetNameNames = new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) });
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithPersistentLocalId(new PersistentLocalId(123))
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode("abc")
                .WithNames(streetNameNames)
                .WithPersistentLocalId(456)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    Fixture.Create<StreetNameWasProposedV2>()
                        .WithPersistentLocalId(command.PersistentLocalId),
                    streetNameWasProposedV2)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameNames.First().Name)));
        }

        [Fact]
        public void WithInvalidLanguage_ThenThrowsStreetNameNameLanguageNotSupportedException()
        {
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French)
            });

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(streetNameNames);

            var languageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    Fixture.Create<StreetNameWasProposedV2>())
                .When(command)
                .Throws(new StreetNameNameLanguageIsNotSupportedException(
                    $"The language '{Language.Dutch}' is not an official or facility language of municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void WithOfficialLanguageNotInProposedStreetName_ThenStreetNameNameWasCorrected()
        {
            var previousEmptyName = new StreetNameName("Rue de la Chapelle", Language.French);
            var streetNameNames = new Names([
                new StreetNameName("Kapelstraat", Language.Dutch),
                previousEmptyName
            ]);

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(streetNameNames);

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.Dutch)
                .Build();

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>()
                .WithNames(new Names([new StreetNameName("Kapestraat", Language.Dutch)]));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameNamesWereCorrected(
                        _municipalityId,
                        command.PersistentLocalId,
                        command.StreetNameNames))
                ));
        }

        [Fact]
        public void WithFacilityLanguageNotInProposedStreetName_ThenStreetNameNameWasCorrected()
        {
            var previousEmptyName = new StreetNameName("Rue de la Chapelle", Language.French);
            var streetNameNames = new Names([
                new StreetNameName("Kapelstraat", Language.Dutch),
                previousEmptyName
            ]);

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(streetNameNames);

            var languageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.Dutch)
                .Build();

            var facilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>()
                .WithLanguage(Language.French)
                .ToEvent();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>()
                .WithNames(new Names([new StreetNameName("Kapestraat", Language.Dutch)]));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    facilityLanguageWasAdded,
                    streetNameWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new StreetNameNamesWereCorrected(
                        _municipalityId,
                        command.PersistentLocalId,
                        command.StreetNameNames))
                ));
        }

        [Fact]
        public void WithNoChanges_ThenNothing()
        {
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French)
            });

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(streetNameNames);

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode("abc")
                .WithNames(streetNameNames)
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposedV2)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithOneChange_ThenOnlyOneStreetNameNameWasChanged()
        {
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kaplestraat", Language.Dutch),
                new StreetNameName("Rue d'la Chapelle", Language.French)
            });

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithNisCode("abc")
                .WithNames(streetNameNames)
                .Build();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First())
                .WithStreetNameName(new StreetNameName("Rue de la Chapelle", Language.French));

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(
                    _municipalityId, command.PersistentLocalId, new Names(new[]
                    {
                        new StreetNameName("Rue de la Chapelle", Language.French),
                    })))));
        }

        [Theory]
        [InlineData("")]
        [InlineData("BO")]
        public void WithConflictingHomonymAdditions_ThenStreetNameNameAlreadyExistsExceptionWasThrown(string homonymAddition)
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var homonymAdditions = !string.IsNullOrEmpty(homonymAddition)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAddition, Language.Dutch) })
                : new HomonymAdditions();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithHomonymAdditions(homonymAdditions)
                .Build();

            var streetNameBWasMigrated =  new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithPersistentLocalId(456)
                .WithHomonymAdditions(homonymAdditions)
                .WithNames(new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) }))
                .Build();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameName(Fixture.Create<Names>().First())
                .WithPersistentLocalId(new PersistentLocalId(streetNameBWasMigrated.PersistentLocalId));

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameAWasMigrated,
                    streetNameBWasMigrated)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Theory]
        [InlineData("", "BO")]
        [InlineData("BO", "")]
        [InlineData("BO", "DE")]
        public void WithDifferentHomonymAdditions_ThenStreetNameNamesWereCorrected(string homonymAdditionA, string homonymAdditionB)
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var homonymAdditionsA = !string.IsNullOrEmpty(homonymAdditionA)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionA, Language.Dutch) })
                : new HomonymAdditions();

            var homonymAdditionsB = !string.IsNullOrEmpty(homonymAdditionB)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionB, Language.Dutch) })
                : new HomonymAdditions();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithStatus(StreetNameStatus.Current)
                .WithHomonymAdditions(homonymAdditionsA)
                .Build();

            var streetNameBWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(456)
                .WithNames(new Names(new[] { new StreetNameName("Kaplestraat", Language.Dutch) }))
                .WithHomonymAdditions(homonymAdditionsB)
                .Build();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithStreetNameName(new StreetNameName("kapelstraat", Language.Dutch))
                .WithPersistentLocalId(new PersistentLocalId(streetNameBWasMigrated.PersistentLocalId));

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameAWasMigrated,
                    streetNameBWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(_municipalityId, command.PersistentLocalId, command.StreetNameNames))));
        }

        [Fact]
        public void StateCheck()
        {
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kaplestraat", Language.Dutch),
                new StreetNameName("Rue d'la Chapelle", Language.French)
            });

            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .WithLanguage(Language.French)
                .Build();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                dutchLanguageWasAdded,
                frenchLanguageWasAdded,
                Fixture.Create<StreetNameWasProposedV2>().WithNames(new Names(new Dictionary<Language, string>
                {
                    { Language.Dutch, "Kapelstraat" },
                    { Language.French, "Rue de la Chapelle" }
                }))
            });

            // Act
            aggregate.CorrectStreetNameName(streetNameNames, persistentLocalId);

            // Assert
            var streetName = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            streetName.Names.Should().BeEquivalentTo(streetNameNames);
        }
    }
}
