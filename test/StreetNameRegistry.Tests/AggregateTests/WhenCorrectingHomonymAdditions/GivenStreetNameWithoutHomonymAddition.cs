namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingHomonymAdditions;

using AutoFixture;
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
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new HomonymAdditions
            {
                new StreetNameHomonymAddition("DEF", Language.German)
            },
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
            .Throws(new CannotCorrectNonExistentHomonymAdditionException(Language.German.ToString())));
    }
}
