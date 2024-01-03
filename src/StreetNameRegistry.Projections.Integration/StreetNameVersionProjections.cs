namespace StreetNameRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie straatnaam versie")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameVersionProjections(IOptions<IntegrationOptions> options)
        {

        }
    }
}
