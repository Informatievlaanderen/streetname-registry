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
        }

        [Fact]
        public void ThenStreetNameNameWasCorrected()
        {
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(_municipalityId,
                    command.PersistentLocalId, new Names(new []
                    {
                        new StreetNameName("Kapelstraat", Language.Dutch),
                        new StreetNameName("Rue de la Croix - Rouge", Language.French)
                    })))));
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
            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameName(streetNameNames.First());

            var languageWasAdded = new MunicipalityOfficialLanguageWasAdded(_municipalityId, Language.Dutch);
            ((ISetProvenance)languageWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _municipalityId,
                new NisCode("abc"),
                streetNameNames,
                new PersistentLocalId(123));
            ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    languageWasAdded,
                    Fixture.Create<StreetNameWasProposedV2>(),
                    streetNameWasProposedV2,
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

            var command = Fixture.Create<CorrectStreetNameNames>()
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

            var command = Fixture.Create<CorrectStreetNameNames>()
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

            var command = Fixture.Create<CorrectStreetNameNames>()
                .WithMunicipalityId(_municipalityId)
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
                .Then(new Fact(_streamId, new StreetNameNamesWereCorrected(
                    _municipalityId, command.PersistentLocalId, new Names(new[] { new StreetNameName("rue de la loi", Language.French) })))));
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
                Fixture.Create<StreetNameWasProposedV2>()
            });

            // Act
            aggregate.CorrectStreetNameName(streetNameNames, persistentLocalId);

            // Assert
            var streetName = aggregate.StreetNames.GetByPersistentLocalId(persistentLocalId);
            streetName.Names.Should().BeEquivalentTo(streetNameNames);
        }
    }
}
