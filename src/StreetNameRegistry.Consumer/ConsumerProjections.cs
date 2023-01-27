namespace StreetNameRegistry.Consumer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Infrastructure;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class ConsumerProjections : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;

        public ConsumerProjections(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var projectorRunner = new ProjectorRunner(_lifetimeScope.Resolve<IConnectedProjectionsManager>(), _lifetimeScope.Resolve<ILoggerFactory>());

            return projectorRunner.Start(stoppingToken);
        }
    }
}
