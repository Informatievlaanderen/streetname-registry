namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using FluentAssertions;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : StreetNameRegistryTest
    {
        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
        }

        [Fact]
        public void ThenStreetNameWasRemovedV2()
        {
            var command = Fixture.Create<RemoveStreetName>();
            var municipalityStreamId = new MunicipalityStreamId(Fixture.Create<MunicipalityId>());

            // Act, assert
            Assert(new Scenario()
                .Given(municipalityStreamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<StreetNameWasProposedV2>())
                .When(command)
                .Then(new Fact(municipalityStreamId, new StreetNameWasRemovedV2(command.MunicipalityId,
                   command.PersistentLocalId))));
        }

        [Fact]
        public void WithRemovedStreetName_ThenNone()
        {
            var command = Fixture.Create<RemoveStreetName>();

            // Act, assert
            Assert(new Scenario()
                .Given(new MunicipalityStreamId(Fixture.Create<MunicipalityId>()),
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<StreetNameWasProposedV2>(),
                    Fixture.Create<StreetNameWasRemovedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAddedBuilder(Fixture)
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names { new StreetNameName("Bergstraat", Language.Dutch), })
                .WithHomonymAdditions(new HomonymAdditions(new[] { new StreetNameHomonymAddition("ABC", Language.Dutch), }))
                .Build();

            // Act
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                dutchLanguageWasAdded,
                streetNameWasMigratedToMunicipality
            });

            aggregate.RemoveStreetName(new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId));

            // Assert
            var streetName = aggregate.StreetNames.GetByPersistentLocalId(Fixture.Create<PersistentLocalId>());
            streetName.IsRemoved.Should().BeTrue();
        }
    }
}
