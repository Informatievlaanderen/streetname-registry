namespace StreetNameRegistry.Tests.Bosa
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Bosa;

    public abstract class BosaProjectionTest<TProjection>
        where TProjection : ConnectedProjection<BosaContext>
    {
        protected ConnectedProjectionTest<BosaContext, TProjection> Sut { get; }

        protected BosaProjectionTest()
        {
            Sut = new ConnectedProjectionTest<BosaContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual BosaContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BosaContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BosaContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
