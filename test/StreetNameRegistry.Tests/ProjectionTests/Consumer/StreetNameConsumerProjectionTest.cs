namespace StreetNameRegistry.Tests.ProjectionTests.Consumer
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Consumer;

    public class StreetNameConsumerProjectionTest<TProjection>
        where TProjection : ConnectedProjection<ConsumerContext>, new()
    {
        protected ConnectedProjectionTest<ConsumerContext, TProjection> Sut { get; }

        public StreetNameConsumerProjectionTest()
        {
            Sut = new ConnectedProjectionTest<ConsumerContext, TProjection>(CreateContext, () => new TProjection());
        }

        protected virtual ConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerContext(options);
        }
    }
}
