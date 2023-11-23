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

    public class GivenStreetNameNameWithoutHomonymAdditionAlreadyExists : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenStreetNameNameWithoutHomonymAdditionAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
            Fixture.Customize(new InfrastructureCustomization());

            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Theory]
        [InlineData("HO", "HO")]
        [InlineData("HO", "ho")]
        public void ThenThrowsStreetNameNameAlreadyExistsException(string homonymAddition, string newHomonymAddition)
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithHomonymAdditions(
                    new HomonymAdditions
                    {
                        new(newHomonymAddition, Language.Dutch)
                    })
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new("Bremt", Language.Dutch),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("A", Language.Dutch),
                }))
                .Build();

            var streetNameWasMigratedToMunicipality2 = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithPersistentLocalId(456)
                .WithStatus(StreetNameStatus.Current)
                .WithNames(new Names
                {
                    new ("Bremt", Language.Dutch),
                }).WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition(homonymAddition, Language.Dutch),
                }))
                .Build();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>(),
                    Fixture.Create<MunicipalityBecameCurrent>(),
                    streetNameWasMigratedToMunicipality,
                    streetNameWasMigratedToMunicipality2)
                .When(command)
                .Throws(new StreetNameNameAlreadyExistsException("Bremt")));
        }
    }
}
