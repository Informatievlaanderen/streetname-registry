namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingHomonymAdditions
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameNameWithHomonymAdditionAlreadyExists : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenStreetNameNameWithHomonymAdditionAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
            Fixture.Customize(new InfrastructureCustomization());

            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenStreetNameNameAlreadyExistsException()
        {
            var command = new CorrectStreetNameHomonymAdditions(
                Fixture.Create<MunicipalityId>(),
                new PersistentLocalId(123),
                new HomonymAdditions(),
                new List<Language>() { Language.Dutch },
                Fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                new PersistentLocalId(123),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

            var secondStreetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                new PersistentLocalId(456),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                },
                new HomonymAdditions(),
                true,
                false);
            ((ISetProvenance)secondStreetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigratedToMunicipality,
                    secondStreetNameWasMigratedToMunicipality)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException("Bergstraat")));
        }

        [Fact]
        public void WithNoMatchingLanguage_ThenNone()
        {
            var command = new CorrectStreetNameHomonymAdditions(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new HomonymAdditions(),
                new List<Language>() { Language.Dutch },
                Fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                Fixture.Create<PersistentLocalId>(),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Rue De Montaigne", Language.French),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("QRS", Language.French),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigratedToMunicipality)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithOneDifferentAndOneSameAddition_ThenOnlyOneAdditionWasCorrected()
        {
            var command = new CorrectStreetNameHomonymAdditions(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new HomonymAdditions
                {
                    new StreetNameHomonymAddition("DEF", Language.Dutch),
                    new StreetNameHomonymAddition("SameFrenchAddition", Language.French),
                },
                new List<Language>(),
                Fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                Fixture.Create<PersistentLocalId>(),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                    new StreetNameName("Rue De Montaigne", Language.French),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("SameFrenchAddition", Language.French),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigratedToMunicipality)
                .When(command)
                .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereCorrected(
                    Fixture.Create<MunicipalityId>(),
                    command.PersistentLocalId,
                    new HomonymAdditions
                    {
                        new StreetNameHomonymAddition("DEF", Language.Dutch)
                    }))));
        }

        [Fact]
        public void WithNoCorrections_ThenNone()
        {
            var command = new CorrectStreetNameHomonymAdditions(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new HomonymAdditions
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                },
                new List<Language>(),
                Fixture.Create<Provenance>());

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                Fixture.Create<PersistentLocalId>(),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names
                {
                    new StreetNameName("Bergstraat", Language.Dutch),
                    new StreetNameName("Rue De Montaigne", Language.French),
                },
                new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                }),
                true,
                false);
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigratedToMunicipality)
                .When(command)
                .ThenNone());
        }
    }
}
