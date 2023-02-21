namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingHomonymAdditions;

using System.Collections.Generic;
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
        var command = new RemoveStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new List<Language>() { Language.Dutch },
            Fixture.Create<Provenance>());

        var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<NisCode>(),
            Fixture.Create<StreetNameId>(),
            Fixture.Create<PersistentLocalId>(),
            status,
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
