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
    using Builders;
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
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());
            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

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
                    command.PersistentLocalId, new Names(new[]
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
                .FromFactory(() => new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                    .WithMunicipalityId(_municipalityId)
                    .WithStatus(StreetNameStatus.Current)
                    .WithPrimaryLanguage(Language.Dutch)
                    .WithIsRemoved()
                    .Build())
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
                .FromFactory(() => new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                    .WithMunicipalityId(_municipalityId)
                    .WithStatus(status)
                    .WithPrimaryLanguage(Language.Dutch)
                    .Build())
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
            languageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var firstStreetName = Fixture.Create<StreetNameWasProposedV2>();
            var secondStreetName = new StreetNameWasProposedV2Builder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithNisCode(new NisCode("abc"))
                .WithNames(streetNameNames)
                .WithPersistentLocalId(firstStreetName.PersistentLocalId + 1)
                .Build();

            // Act, assert

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    firstStreetName,
                    secondStreetName,
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
            languageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithNisCode(new NisCode("abc"))
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

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            languageWasAdded.SetProvenance(Fixture.Create<Provenance>());

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

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(streetNameNames);

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithNisCode(new NisCode("abc"))
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
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French)
            });

            var command = Fixture.Create<ChangeStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names())
                .WithStreetNameName(streetNameNames.First())
                .WithStreetNameName(new StreetNameName("rue de la loi", Language.French));

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithNisCode(new NisCode("abc"))
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

            var homonymAdditions = !string.IsNullOrEmpty(homonymAddition)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAddition, Language.Dutch) })
                : new HomonymAdditions();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithPersistentLocalId(123)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .WithHomonymAdditions(homonymAdditions)
                .Build();

            var streetNameBWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithPersistentLocalId(456)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .WithNames(new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) }))
                .WithHomonymAdditions(homonymAdditions)
                .Build();

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

            var homonymAdditionsA = !string.IsNullOrEmpty(homonymAdditionA)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionA, Language.Dutch) })
                : new HomonymAdditions();
            var homonymAdditionsB = !string.IsNullOrEmpty(homonymAdditionB)
                ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(homonymAdditionB, Language.Dutch) })
                : new HomonymAdditions();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameAWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithPersistentLocalId(123)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .WithHomonymAdditions(homonymAdditionsA)
                .Build();

            var streetNameBWasMigrated = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithMunicipalityId(_municipalityId)
                .WithPersistentLocalId(456)
                .WithStatus(StreetNameStatus.Current)
                .WithPrimaryLanguage(Language.Dutch)
                .WithNames(new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) }))
                .WithHomonymAdditions(homonymAdditionsB)
                .Build();

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
            var streetNameNames = new Names(new[]
            {
                new StreetNameName("Kapelstraat", Language.Dutch),
                new StreetNameName("Rue de la Chapelle", Language.French)
            });

            var persistentLocalId = Fixture.Create<PersistentLocalId>();
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            dutchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

            var frenchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.French);
            frenchLanguageWasAdded.SetProvenance(Fixture.Create<Provenance>());

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
