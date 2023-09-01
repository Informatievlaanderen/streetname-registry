namespace StreetNameRegistry.Tests.AggregateTests.WhenChangingStreetNameName
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
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

            Fixture.Register(() => new Names(Fixture.Create<Dictionary<Language, string>>()));
        }

        [Fact]
        public void ThenStreetNameNameWasChanged()
        {
            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names())
                .WithStreetNameName(new StreetNameName("Kapelstraat", Language.Dutch))
                .WithStreetNameName(new StreetNameName("Rue de la Croix - Rouge", Language.French));

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance)frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    Fixture.Create<StreetNameWasProposedV2>())
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereChanged(_municipalityId,
                    command.PersistentLocalId, new Names(new []
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la Croix - Rouge", Language.French)
                    })))));
        }

        [Fact]
        public void ThenThrowsStreetNameNotFoundException()
        {
            var command = Fixture.Create<ChangeStreetNameNames>()
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
            var command = Fixture.Create<ChangeStreetNameNames>()
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

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
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
            var command = Fixture.Create<ChangeStreetNameNames>()
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

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
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
            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2_1 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasProposedV2_2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                new PersistentLocalId(streetNameWasProposedV2_1.PersistentLocalId + 1));
            ((ISetProvenance)streetNameWasProposedV2_2).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    streetNameWasProposedV2_1,
                    streetNameWasProposedV2_2,
                    Fixture.Create<StreetNameWasRejected>())
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException(command.PersistentLocalId)));
        }

        [Fact]
        public void WithStreetNameNameAlreadyInUse_ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var streetNameNames = new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) });
            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithPersistentLocalId(new PersistentLocalId(123))
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                new PersistentLocalId(456));
            ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

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
            var streetNameNames = new Names(new[] {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French) });

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance)languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

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
            var streetNameNames = new Names(new[] {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French) });

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance)frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                Fixture.Create<PersistentLocalId>());
            ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

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
            var streetNameNames = new Names(new[] {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French) });

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First())
                .WithStreetNameName(new StreetNameName("rue de la loi", Language.French));

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance)frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                Fixture.Create<PersistentLocalId>());
            ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    dutchLanguageWasAdded,
                    frenchLanguageWasAdded,
                    streetNameWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameNamesWereChanged(
                    _municipalityId, command.PersistentLocalId, new Names(new[] { new StreetNameName("rue de la loi", Language.French) })))));
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

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
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

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<ChangeStreetNameNames>()
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
        public void WithDifferentHomonymAdditions_ThenStreetNameNamesWereChanged(string homonymAdditionA, string homonymAdditionB)
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

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
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
                        !string.IsNullOrEmpty(homonymAdditionB)
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionB, Language.Dutch) })
                            : new HomonymAdditions(),
                        true,
                        false);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<ChangeStreetNameNames>()
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
                .Then(new Fact(_streamId, new StreetNameNamesWereChanged(_municipalityId, command.PersistentLocalId, command.StreetNameNames))));
        }

        [Fact]
        public void StateCheck()
        {
            var streetNameNames = new Names(new[] {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French) });

            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            ((ISetProvenance)frenchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                dutchLanguageWasAdded,
                frenchLanguageWasAdded,
                Fixture.Create<StreetNameWasProposedV2>().WithNames(new Names(new Dictionary<Language, string>
                {
                    { Language.Dutch, Fixture.Create<string>() },
                    { Language.French, Fixture.Create<string>() }
                }))
            });

            // Act
            aggregate.ChangeStreetNameName(streetNameNames, persistentLocalId);

            // Assert
            var streetName = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            streetName.Names.Should().BeEquivalentTo(streetNameNames);
        }
    }
}
