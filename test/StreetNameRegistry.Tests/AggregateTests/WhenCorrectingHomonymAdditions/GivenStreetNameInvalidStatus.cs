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

    public class GivenStreetNameInvalidStatus : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenStreetNameInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
            Fixture.Customize(new InfrastructureCustomization());

            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus status)
        {
            var command = new CorrectStreetNameHomonymAdditionsBuilder(Fixture)
                .WithHomonymAdditions(new HomonymAdditions
                {
                    new("DEF", Language.Dutch)
                })
                .Build();

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(Fixture)
                .WithStatus(status)
                .WithPrimaryLanguage(Language.Dutch)
                .WithNames(
                    new Names
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
                .Throws(new StreetNameHasInvalidStatusException()));
        }
    }
}
