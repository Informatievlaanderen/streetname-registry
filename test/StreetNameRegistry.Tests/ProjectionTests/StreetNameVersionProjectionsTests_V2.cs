namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Municipality;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Xunit;

    public class StreetNameVersionProjectionsTests_V2 : ConnectedProjection<IntegrationContext>
    {
        private readonly Fixture? _fixture;

        public StreetNameVersionProjectionsTests_V2()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        private static string? DetermineExpectedNameForLanguage(IDictionary<Language, string> streetNameNames, Language language)
            => streetNameNames.ContainsKey(language) ? streetNameNames[language] : null;

        [Fact]
        public async Task WhenStreetNameWasProposedV2_ThenNewStreetNameWasAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));

            var streetNameWasProposedV2 = _fixture.Create<StreetNameRegistry.Municipality.Events.StreetNameWasProposedV2>();
            var position = 123L;
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, streetNameWasProposedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var persistentLocalId = 123;
            var mockLegacyIdToPersistentLocalIdMapper = new Mock<ILegacyIdToPersistentLocalIdMapper>();
            mockLegacyIdToPersistentLocalIdMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(persistentLocalId);

            var sut = new ConnectedProjectionTest<IntegrationContext, StreetNameVersionProjections>(
                CreateContext,
                () => new StreetNameVersionProjections(Options.Create(new IntegrationOptions
                {
                    Namespace = ""
                }), mockLegacyIdToPersistentLocalIdMapper.Object));

            await sut
                .Given(new Envelope<StreetNameRegistry.Municipality.Events.StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expected = await ct.FindAsync<StreetNameVersion>(position);
                    expected.Should().NotBeNull();
                    expected.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expected.PersistentLocalId.Should().Be(streetNameWasProposedV2.PersistentLocalId);
                    expected.IsRemoved.Should().BeFalse();
                    expected.Status.Should().Be("voorgesteld");
                    expected.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expected.NameDutch.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.Dutch));
                    expected.NameFrench.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.French));
                    expected.NameGerman.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.German));
                    expected.NameEnglish.Should().Be(DetermineExpectedNameForLanguage(streetNameWasProposedV2.StreetNameNames, Language.English));
                });
        }

        protected virtual IntegrationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<IntegrationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new IntegrationContext(options);
        }
    }
}
