namespace StreetNameRegistry.Tests.AggregateTests.WhenImportingMunicipality
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenNoMunicipality : StreetNameRegistryTest
    {
        public GivenNoMunicipality(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenMunicipalityWasImported()
        {
            var command = Fixture.Create<ImportMunicipality>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new MunicipalityStreamId(command.MunicipalityId),
                    new MunicipalityWasImported(command.MunicipalityId, command.NisCode))));
        }

        [Fact]
        public void StateCheck()
        {
            var nisCode = Fixture.Create<NisCode>();
            var municipalityId = Fixture.Create<MunicipalityId>();

            // Act
            var aggregate = Municipality.Register(
                new MunicipalityFactory(NoSnapshotStrategy.Instance),
                municipalityId,
                nisCode);

            // Assert
            aggregate.MunicipalityId.Should().Be(municipalityId);
            aggregate.NisCode.Should().Be(nisCode);
            aggregate.MunicipalityStatus.Should().Be(MunicipalityStatus.Proposed);
        }
    }
}
