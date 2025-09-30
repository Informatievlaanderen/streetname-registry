namespace StreetNameRegistry.Tests.ProjectionTests.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Projections.Legacy;

    public abstract class StreetNameLegacyProjectionTest<TProjection>
        where TProjection : ConnectedProjection<LegacyContext>, new()
    {
        protected ConnectedProjectionTest<LegacyContext, TProjection> Sut { get; }

        protected StreetNameLegacyProjectionTest()
        {
            Sut = new ConnectedProjectionTest<LegacyContext, TProjection>(CreateContext, () => new TProjection());
        }

        protected virtual LegacyContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<LegacyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LegacyContext(options);
        }
    }
}
