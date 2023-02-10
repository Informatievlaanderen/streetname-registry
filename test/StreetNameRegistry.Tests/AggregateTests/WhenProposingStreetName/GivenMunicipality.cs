namespace StreetNameRegistry.Tests.AggregateTests.WhenProposingStreetName
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
                .Then(new[]
                {
                    new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))
                }));
        }

        [Fact]
        public void WithExistingStreetName_ThenStreetNameNameAlreadyExistsExceptionWasThrown()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    streetNameWasProposed)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException(streetNameName.Name)));
        }

        [Fact]
        public void WithExistingPersistentLocalId_ThenThrowsStreetNamePersistentLocalIdAlreadyExistsException()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
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

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))));
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

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))));
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
            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
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
                        true);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))));
        }

        [Fact]
        public void WithOneExistingStreetNameAndOneNew_ThenStreetNameNameAlreadyExistsExceptionWasThrown()
        {
            var existingStreetNameName = Fixture.Create<StreetNameName>();
            var newStreetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { existingStreetNameName });

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var streetNameWasProposed = Fixture.Create<StreetNameWasProposedV2>();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
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
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(new Names { newStreetNameName });

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasProposed)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))
                }));
        }

        [Fact]
        public void WithMunicipalityRetired_ThenMunicipalityHasInvalidStatusExceptionWasThrown()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityWasRetired = Fixture.Create<MunicipalityWasRetired>();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
                .WithRandomStreetName(Fixture);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityWasRetired)
                .When(command)
                .Throws(new MunicipalityHasInvalidStatusException($"Municipality with id '{_municipalityId}' was retired")));
        }

        [Fact]
        public void WithOfficialLanguageDutchAndProposedLanguageIsFrench_ThenStreetNameNameLanguageNotSupportedExceptionWasThrown()
        {
            Fixture.Register(() => Language.Dutch);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();

            var names = new Names
            {
                new StreetNameName(Fixture.Create<string>(), Language.French)
            };

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(names);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded)
                .When(command)
                .Throws(new StreetNameNameLanguageIsNotSupportedException($"The language '{Language.French}' is not an official or facility language of municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void WithFacilityLanguageFrenchAndProposedLanguageIsDutch_ThenStreetNameNameLanguageNotSupportedExceptionWasThrown()
        {
            Fixture.Register(() => Language.French);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityFacilityLanguageWasAdded = Fixture.Create<MunicipalityFacilityLanguageWasAdded>();

            var names = new Names
            {
                new StreetNameName(Fixture.Create<string>(), Language.Dutch)
            };

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
                .WithStreetNameNames(names);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityFacilityLanguageWasAdded)
                .When(command)
                .Throws(new StreetNameNameLanguageIsNotSupportedException($"The language '{Language.Dutch}' is not an official or facility language of municipality '{_municipalityId}'.")));
        }

        [Fact]
        public void WithOfficialLanguageDutchAndFacilityLanguageFrenchAndProposedLanguageIsFrench_ThenStreetNameNameLanguageNotSupportedExceptionWasThrown()
        {
            Fixture.Register(() => Language.Dutch);
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityDutchOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            Fixture.Register(() => Language.French);
            var municipalityFacilityLanguageWasAdded = Fixture.Create<MunicipalityFacilityLanguageWasAdded>();

            var names = new Names
            {
                new StreetNameName(Fixture.Create<string>(), Language.French)
            };

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId)
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
        public void WithExistingStreetNameAndNoHomonymAdditions_ThenStreetNameNameAlreadyExistsExceptionWasThrown()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => Language.Dutch);
            Fixture.Register(() => Taal.NL);

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityOfficialLanguageWasAdded = Fixture.Create<MunicipalityOfficialLanguageWasAdded>();
            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
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
                        false);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

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
            var streetNameWasMigrated = Fixture.Build<StreetNameWasMigratedToMunicipality>()
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
                        new HomonymAdditions(){new StreetNameHomonymAddition("test", Language.Dutch)},
                        true,
                        false);

                    ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
                    return streetNameWasMigratedToMunicipality;
                })
                .Create();

            var command = Fixture.Create<ProposeStreetName>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    municipalityWasImported,
                    municipalityOfficialLanguageWasAdded,
                    streetNameWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameWasProposedV2(_municipalityId, new NisCode(municipalityWasImported.NisCode), command.StreetNameNames, command.PersistentLocalId))));
        }
    }
}
