namespace StreetNameRegistry.Tests.AggregateTests.WhenSettingMunicipalityToCurrent
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

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityBecameCurrent()
        {
            var command = Fixture.Create<SetMunicipalityToCurrent>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityBecameCurrent(command.MunicipalityId))));
        }

        [Fact]
        public void WithCurrentMunicipality_ThenNone()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityBecameCurrent = Fixture.Create<MunicipalityBecameCurrent>();
            var command = Fixture.Create<SetMunicipalityToCurrent>();

            Assert(new Scenario()
                .Given(
                    _streamId,
                    municipalityWasImported,
                    municipalityBecameCurrent
                )
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>()
            });

            // Act
            aggregate.BecomeCurrent();

            // Assert
            aggregate.MunicipalityStatus.Should().Be(MunicipalityStatus.Current);
        }
    }
}
