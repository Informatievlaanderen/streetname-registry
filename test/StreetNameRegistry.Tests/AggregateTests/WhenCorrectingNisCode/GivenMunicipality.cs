namespace StreetNameRegistry.Tests.AggregateTests.WhenCorrectingNisCode
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

    public class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityNisCodeWasChanged()
        {
            var command = Fixture.Create<CorrectMunicipalityNisCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityNisCodeWasChanged(command.MunicipalityId, command.NisCode, []))));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var newNisCode = Fixture.Create<NisCode>();

            aggregate.Initialize(new List<object>
            {
                municipalityWasImported
            });

            // Act
            aggregate.DefineOrChangeNisCode(newNisCode);

            // Assert
            municipalityWasImported.NisCode.Should().NotBe(newNisCode);
            aggregate.NisCode.Should().Be(newNisCode);
        }
    }
}
