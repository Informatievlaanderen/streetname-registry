namespace StreetNameRegistry.Tests.ProjectionTests.Consumer
{
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetNameRegistry.Consumer.Projections;
    using StreetNameRegistry.Municipality.Events;
    using Xunit;

    public sealed class StreetNameConsumerProjectionsTests : StreetNameConsumerProjectionTest<MunicipalityConsumerProjection>
    {
        private readonly Fixture _fixture;

        public StreetNameConsumerProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public Task MunicipalityWasImportedAddsMunicipality()
        {
            var municipalityWasImported = _fixture.Create<MunicipalityWasImported>();

            return Sut
                .Given(municipalityWasImported)
                .Then(async ct =>
                {
                    var expectedMunicipality = await ct.MunicipalityConsumerItems.FindAsync(municipalityWasImported.MunicipalityId);
                    expectedMunicipality.Should().NotBeNull();
                    expectedMunicipality.MunicipalityId.Should().Be(municipalityWasImported.MunicipalityId);
                    expectedMunicipality.NisCode.Should().Be(municipalityWasImported.NisCode);
                });
        }

        [Fact]
        public Task MunicipalityNisCodeWasChangedSetsNisCode()
        {
            var municipalityWasImported = _fixture.Create<MunicipalityWasImported>();
            var municipalityNisCodeWasChanged = _fixture.Create<MunicipalityNisCodeWasChanged>();

            return Sut
                .Given(municipalityWasImported, municipalityNisCodeWasChanged)
                .Then(async ct =>
                {
                    var expectedMunicipality = await ct.MunicipalityConsumerItems.FindAsync(municipalityWasImported.MunicipalityId);
                    expectedMunicipality.Should().NotBeNull();
                    expectedMunicipality.MunicipalityId.Should().Be(municipalityWasImported.MunicipalityId);
                    expectedMunicipality.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);
                });
        }
    }
}
