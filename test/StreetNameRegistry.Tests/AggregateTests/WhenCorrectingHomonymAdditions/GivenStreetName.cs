namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingHomonymAdditions
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
            Fixture.Customize(new InfrastructureCustomization());

            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void WithDifferentAdditions_ThenStreetNameHomonymAdditionsCorrected()
        {
            var homonymAdditions = new HomonymAdditions { new("DEF", Language.Dutch) };
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(homonymAdditions)
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
                .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereCorrected(
                    Fixture.Create<MunicipalityId>(),
                    command.PersistentLocalId,
                    homonymAdditions))));
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
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
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

        [Theory]
        [InlineData(StreetNameStatus.Current)]
        [InlineData(StreetNameStatus.Proposed)]
        public void WithValidStatus_ThenStreetNameHomonymAdditionsCorrected(StreetNameStatus status)
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(new HomonymAdditions
                {
                    new("DEF", Language.Dutch)
                })
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(status)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
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
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();
            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names { new("Bergstraat", Language.Dutch) })
                .WithHomonymAdditions(new HomonymAdditions(new[] { new StreetNameHomonymAddition("ABC", Language.Dutch) }))
                .Build();

            var streetNameHomonymAdditionWasCorrected = new StreetNameHomonymAdditionsWereCorrectedBuilder(Fixture)
                    .WithHomonymAdditions(new List<StreetNameHomonymAddition> { new("DEF", Language.Dutch) })
                    .Build();

            // Act
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                dutchLanguageWasAdded,
                streetNameWasMigratedToMunicipality,
                streetNameHomonymAdditionWasCorrected
            });

            // Assert
            var streetName = aggregate.StreetNames.GetByPersistentLocalId(Fixture.Create<PersistentLocalId>());
            streetName.HomonymAdditions.Single().Language.Should().Be(Language.Dutch);
            streetName.HomonymAdditions.Single().HomonymAddition.Should().Be("DEF");
        }
    }
}
