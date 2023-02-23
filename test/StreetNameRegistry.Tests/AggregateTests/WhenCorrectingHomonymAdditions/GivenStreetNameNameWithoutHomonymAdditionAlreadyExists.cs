namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingHomonymAdditions;

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

    [Fact]
    public void ThenThrowsStreetNameNameAlreadyExistsException()
    {
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            new PersistentLocalId(123),
            new HomonymAdditions
            {
                new StreetNameHomonymAddition("B", Language.Dutch)
            },
            new List<Language>(),
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
                new StreetNameName("Bremt", Language.Dutch),
            },
            new HomonymAdditions(new[]
            {
                new StreetNameHomonymAddition("A", Language.Dutch),
            }),
            true,
            false);
        ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

        var streetNameWasMigratedToMunicipality2 = new StreetNameWasMigratedToMunicipality(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<NisCode>(),
            Fixture.Create<StreetNameId>(),
            new PersistentLocalId(456),
            StreetNameStatus.Current,
            Language.Dutch,
            null,
            new Names
            {
                new StreetNameName("Bremt", Language.Dutch),
            },
            new HomonymAdditions(new[]
            {
                new StreetNameHomonymAddition("B", Language.Dutch),
            }),
            true,
            false);
        ((ISetProvenance)streetNameWasMigratedToMunicipality2).SetProvenance(Fixture.Create<Provenance>());

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
