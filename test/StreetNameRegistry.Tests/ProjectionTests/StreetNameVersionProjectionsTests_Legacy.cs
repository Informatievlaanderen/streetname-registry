namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.Legacy.StreetNameDetailV2;

    public class StreetNameVersionProjectionsTests : ConnectedProjection<IntegrationContext>
    {
        private readonly Fixture? _fixture;

        public StreetNameVersionProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        public void T()
        {
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

            var streetNameId = Guid.NewGuid().ToString("D");
            var municipalityId = Guid.NewGuid().ToString("D");

            var streetNameWasRegistered = new StreetNameWasRegistered(streetNameId, municipalityId, "1234", _fixture.Create<Provenance>());

            // var metadata = new Dictionary<string, object>
            // {
            //     { AddEventHashPipe.HashMetadataKey, streetNameWasRegistered.GetHash() }
            // };

            // await GivenEvents()
            //     .Project(Generate.StreetNameWasRegistered
            //         .Select(e => e.WithId(id)
            //             .WithProvenance(provenance)))
            //     .Then(async ct => (await ct.FindAsync<StreetNameListItem>(id)).Should().NotBeNull());
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
