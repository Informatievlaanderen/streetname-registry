namespace StreetNameRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using Municipality.Events;
    using Projections.Integration.Merger;
    using Xunit;

    public class StreetNameMergerItemProjectionTests : IntegrationProjectionTest<StreetNameMergerItemProjections>
    {
        private readonly Fixture _fixture;

        public StreetNameMergerItemProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPersistentLocalId());
            _fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenStreetNameWasProposedForMunicipalityMerger()
        {
            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMergerBuilder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                    new StreetNameHomonymAddition("AZE", Language.English),
                    new StreetNameHomonymAddition("QSD", Language.German),
                }))
                .Build();

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<StreetNameWasProposedForMunicipalityMerger>(new Envelope(streetNameWasProposedForMunicipalityMerger, firstEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameMergerItems.FirstOrDefaultAsync(x => x.NewPersistentLocalId == streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.NewPersistentLocalId.Should().Be(streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    expectedLatestItem.MergedPersistentLocalId.Should()
                        .Be(streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.First());
                });
        }

        protected override StreetNameMergerItemProjections CreateProjection()
            => new StreetNameMergerItemProjections();
    }
}
