namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingHomonymAdditions;

using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using global::AutoFixture;
using Municipality;
using Municipality.Commands;
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
    public void WithDifferentAddition_ThenStreetNameHomonymAdditionsCorrectedEvent()
    {
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new HomonymAdditions
            {
                new StreetNameHomonymAddition("DEF", Language.Dutch)
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
            .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereCorrected(
                Fixture.Create<MunicipalityId>(),
                command.PersistentLocalId,
                new HomonymAdditions
                {
                    new StreetNameHomonymAddition("DEF", Language.Dutch)
                }))));
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

    [Theory]
    [InlineData(StreetNameStatus.Current)]
    [InlineData(StreetNameStatus.Proposed)]
    public void WithValidStatus_ThenStreetNameHomonymAdditionsCorrectedEvent(StreetNameStatus status)
    {
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new HomonymAdditions
            {
                new StreetNameHomonymAddition("DEF", Language.Dutch)
            },
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
            .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereCorrected(
                Fixture.Create<MunicipalityId>(),
                command.PersistentLocalId,
                new HomonymAdditions
                {
                    new StreetNameHomonymAddition("DEF", Language.Dutch)
                }))));
    }

    [Fact]
    public void StateCheck()
    {
        var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

        var dutchLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(Fixture.Create<MunicipalityId>(), Language.Dutch);
        ((ISetProvenance)dutchLanguageWasAdded).SetProvenance(Fixture.Create<Provenance>());

        var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<NisCode>(),
            Fixture.Create<StreetNameId>(),
            Fixture.Create<PersistentLocalId>(),
            StreetNameStatus.Current,
            Language.Dutch,
            null,
            new Names { new StreetNameName("Bergstraat", Language.Dutch), },
            new HomonymAdditions(new[] { new StreetNameHomonymAddition("ABC", Language.Dutch), }),
            true,
            false);
        ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());

        var streetNameHomonymAdditionWasCorrected = new StreetNameHomonymAdditionsWereCorrected(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new List<StreetNameHomonymAddition>() { new StreetNameHomonymAddition("DEF", Language.Dutch) });
        ((ISetProvenance)streetNameHomonymAdditionWasCorrected).SetProvenance(Fixture.Create<Provenance>());

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
