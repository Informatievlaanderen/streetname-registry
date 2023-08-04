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
            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposed = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("1011"),
                new Names(new List<StreetNameName>()
                {
                    new StreetNameName("Kaplestraat", Language.Dutch),
                    new StreetNameName("Rue d'la Croix - Rouge", Language.French),
                }),
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposed.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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
                    command.PersistentLocalId,  command.StreetNameNames))
                ));
        }

        [Fact]
        public void WhenCorrectionExceedsLevenshteinThreshold_ThrowException()
        {
            var originalName = "0123546789";
            var correctedName = "012354----";

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposed = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("1011"),
                new Names(new List<StreetNameName>()
                {
                    new StreetNameName(originalName, Language.Dutch)
                }),
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposed.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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
        public void WhenCorrectionExactly30Percent_ThenStreetNameWasCorrected()
        {
            var originalName = "0123456789";
            var correctedName = "0123456---";

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposed = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("1011"),
                new Names(new List<StreetNameName>()
                {
                    new StreetNameName(originalName, Language.Dutch)
                }),
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposed.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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
                    command.PersistentLocalId,  command.StreetNameNames))
                ));
        }

        [Fact]
        public void ThenThrowsStreetNameNotFoundException()
        {
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, municipalityWasImported)
                .When(command)
                .Throws(new StreetNameIsNotFoundException(command.PersistentLocalId)));
        }

        [Fact]
        public void ThenThrowsStreetNameIsRemovedException()
        {
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var removedStreetNameMigratedToMunicipality = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        Fixture.Create<PersistentLocalId>(),
                        StreetNameStatus.Current,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        new HomonymAdditions(),
                        true,
                        isRemoved: true);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();


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
        public void ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameMigratedToMunicipality = Fixture.Build<StreetNameWasMigratedToMunicipality>()
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
                        true,
                        isRemoved: false);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();


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
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance) languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                new PersistentLocalId(command.PersistentLocalId + 1));
            ((ISetProvenance) streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

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
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance) languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                new PersistentLocalId(456));
            ((ISetProvenance) streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

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
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance) languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

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
        public void WithNoChanges_ThenNothing()
        {
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French)
            });

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance) dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance) frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                Fixture.Create<PersistentLocalId>());
            ((ISetProvenance) streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

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

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                Fixture.Create<PersistentLocalId>());
            streetNameWasProposedV2.SetProvenance(Fixture.Create<Provenance>());

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        new PersistentLocalId(123),
                        StreetNameStatus.Current,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        !string.IsNullOrEmpty(homonymAddition)
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAddition, Language.Dutch) })
                            : new HomonymAdditions(),
                        true,
                        false);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var streetNameBWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        new PersistentLocalId(456),
                        StreetNameStatus.Current,
                        Language.Dutch,
                        null,
                        new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) }),
                        !string.IsNullOrEmpty(homonymAddition)
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAddition, Language.Dutch) })
                            : new HomonymAdditions(),
                        true,
                        false);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        new PersistentLocalId(123),
                        StreetNameStatus.Current,
                        Language.Dutch,
                        null,
                        Fixture.Create<Names>(),
                        !string.IsNullOrEmpty(homonymAdditionA)
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionA, Language.Dutch) })
                            : new HomonymAdditions(),
                        true,
                        false);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var streetNameBWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
                .FromFactory(() =>
                {
                    var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                        _municipalityId,
                        Fixture.Create<NisCode>(),
                        Fixture.Create<StreetNameId>(),
                        new PersistentLocalId(456),
                        StreetNameStatus.Current,
                        Language.Dutch,
                        null,
                        new Names(new[] { new StreetNameName("Kaplestraat", Language.Dutch) }),
                        !string.IsNullOrEmpty(homonymAdditionB)
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionB, Language.Dutch) })
                            : new HomonymAdditions(),
                        true,
                        false);

                    ((ISetProvenance) streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance) dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance) frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

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
