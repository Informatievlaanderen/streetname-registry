namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingHomonymAdditions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameWithoutHomonymAddition : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenStreetNameWithoutHomonymAddition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
            Fixture.Customize(new InfrastructureCustomization());

            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenThrowsCannotCorrectNonExistentHomonymAdditionException()
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(new HomonymAdditions
                {
                    new("DEF", Language.German)
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
                .Throws(new CannotAddHomonymAdditionException(Language.German)));
        }
    }
}
