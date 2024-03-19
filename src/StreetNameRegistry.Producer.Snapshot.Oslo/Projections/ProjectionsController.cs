namespace StreetNameRegistry.Producer.Snapshot.Oslo.Projections
{
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Controllers;
    using Microsoft.Extensions.Configuration;
    using StreetNameRegistry.Infrastructure;

    [ApiVersion("1.0")]
    [ApiRoute("projections")]
    public class ProjectionsController : DefaultProjectorController
    {
        public ProjectionsController(
            IConnectedProjectionsManager connectedProjectionsManager,
            IConfiguration configuration)
            : base(
                connectedProjectionsManager,
                configuration.GetValue<string>("BaseUrl"))
        {
            RegisterConnectionString(Schema.ProducerSnapshotOslo, configuration.GetConnectionString("ProducerProjections"));
        }
    }
}
