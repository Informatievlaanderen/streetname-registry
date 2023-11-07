namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingHomonymAdditions
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using global::AutoFixture;
    using Municipality;
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
        public void ThenThrowsStreetNameNameAlreadyExistsException()
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithHomonymAdditionsToRemove(new List<Language> { Language.Dutch })
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                }))
                .Build();

            var secondStreetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(456)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                })
                .Build();

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
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Rue De Montaigne", Language.French),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("QRS", Language.French),
                }))
                .Build();

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
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(new HomonymAdditions
                {
                    new("DEF", Language.Dutch),
                    new("SameFrenchAddition", Language.French),
                }).Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                }).WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("SameFrenchAddition", Language.French),
                }))
                .Build();

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
                        new("DEF", Language.Dutch)
                    }))));
        }

        [Fact]
        public void WithNoCorrections_ThenNone()
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(new HomonymAdditions
                {
                    new("ABC", Language.Dutch),
                    new("DEF", Language.French),
                })
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                }))
                .Build();

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
