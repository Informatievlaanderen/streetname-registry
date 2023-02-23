namespace StreetNameRegistry.Tests.AggregateTests.WhenRemovingHomonymAdditions;

using System.Collections.Generic;
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
    public void WithSameLanguage_ThenStreetNameHomonymAdditionsWereRemoved()
    {
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new HomonymAdditions(),
            new List<Language>() { Language.Dutch },
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
            .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereRemoved(
                Fixture.Create<MunicipalityId>(),
                command.PersistentLocalId,
                new List<Language>(){ Language.Dutch }))));
    }

    [Fact]
    public void WithNoMatchingLanguage_ThenNone()
    {
        var command = new CorrectStreetNameHomonymAdditions(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new HomonymAdditions(),
            new List<Language>() { Language.Dutch },
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
                new StreetNameName("Rue De Montaigne", Language.French),
            },
            new HomonymAdditions(new[]
            {
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
            new HomonymAdditions(),
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
            .Then(new Fact(_streamId, new StreetNameHomonymAdditionsWereRemoved(
                Fixture.Create<MunicipalityId>(),
                command.PersistentLocalId,
                new List<Language>() { Language.Dutch }))));
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

        var homonymAdditionsWereRemoved = new StreetNameHomonymAdditionsWereRemoved(
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<PersistentLocalId>(),
            new List<Language>() { Language.Dutch} );
        ((ISetProvenance)homonymAdditionsWereRemoved).SetProvenance(Fixture.Create<Provenance>());

        // Act
        aggregate.Initialize(new List<object>
        {
            Fixture.Create<MunicipalityWasImported>(),
            Fixture.Create<MunicipalityBecameCurrent>(),
            dutchLanguageWasAdded,
            streetNameWasMigratedToMunicipality,
            homonymAdditionsWereRemoved
        });

        // Assert
        var streetName = aggregate.StreetNames.GetByPersistentLocalId(Fixture.Create<PersistentLocalId>());
        streetName.HomonymAdditions.Should().BeEmpty();
    }
}
