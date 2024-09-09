namespace StreetNameRegistry.Tests.AggregateTests.WhenRetiringMunicipality
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Extensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Events;
    using Municipality.Exceptions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipality : StreetNameRegistryTest
    {
        private readonly MunicipalityId _municipalityId;
        private readonly MunicipalityStreamId _streamId;

        public GivenMunicipality(ITestOutputHelper output) : base(output)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            _municipalityId = Fixture.Create<MunicipalityId>();
            _streamId = Fixture.Create<MunicipalityStreamId>();
        }

        [Fact]
        public void ThenMunicipalityWasRetired()
        {
            var command = Fixture.Create<RetireMunicipality>()
                .WithMunicipalityId(_municipalityId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MunicipalityWasImported>())
                .When(command)
                .Then(new Fact(_streamId, new MunicipalityWasRetired(command.MunicipalityId))));
        }

        [Fact]
        public void WithRetiredMunicipality_ThenNone()
        {
            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();
            var municipalityWasRetired = Fixture.Create<MunicipalityWasRetired>();
            var command = Fixture.Create<RetireMunicipality>()
                .WithMunicipalityId(new MunicipalityId(_municipalityId));

            Assert(new Scenario()
                .Given(
                    _streamId,
                    municipalityWasImported,
                    municipalityWasRetired
                )
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithActiveStreetNames_ThenThrowsStreetNamesAreActiveException()
        {
            var command = Fixture.Create<RetireMunicipality>();

            var streetNamePersistentLocalIds = Fixture.CreateMany<PersistentLocalId>(5).ToList();

            // Act, assert
            Assert(new Scenario()
                .Given(_streamId, new object[]
                    {
                        Fixture.Create<MunicipalityWasImported>(),
                        Fixture.Create<MunicipalityBecameCurrent>()
                    }
                    .Concat(streetNamePersistentLocalIds.SelectMany(persistentLocalId => new object[]
                        {
                            Fixture.Create<StreetNameWasProposedV2>().WithPersistentLocalId(persistentLocalId),
                            Fixture.Create<StreetNameWasApproved>().WithPersistentLocalId(persistentLocalId)
                        })
                    )
                    .ToArray())
                .When(command)
                .Throws(new StreetNamesAreActiveException()));
        }

        [Fact]
        public void StateCheck()
        {
            var aggregate = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>()
            });

            // Act
            aggregate.Retire();

            // Assert
            aggregate.MunicipalityStatus.Should().Be(MunicipalityStatus.Retired);
        }
    }

    public static class RetireMunicipalityExtensions
    {
        public static RetireMunicipality WithMunicipalityId(
            this RetireMunicipality command,
            MunicipalityId municipalityId)
        {
            return new RetireMunicipality(municipalityId, new RetirementDate(command.RetirementDate), command.Provenance);
        }
    }
}
