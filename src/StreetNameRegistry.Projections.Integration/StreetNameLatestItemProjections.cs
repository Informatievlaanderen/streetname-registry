namespace StreetNameRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie straatnaam latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameLatestItemProjections(IOptions<IntegrationOptions> options)
        {

        }
    }
}
