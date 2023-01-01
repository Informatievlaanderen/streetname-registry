namespace StreetNameRegistry.Consumer.Microsoft.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;

    public class CommandHandler
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(IServiceProvider services, ILogger<CommandHandler> logger)
        {
            _services = services;
            _logger = logger;
        }

        public virtual async Task Handle<T>(T command, CancellationToken cancellationToken)
            where T : class, IHasCommandProvenance
        {
            _logger.LogDebug($"Handling {command.GetType().FullName}");

            await using var scope = _services.CreateAsyncScope();

            var resolver = scope.ServiceProvider.GetRequiredService<ICommandHandlerResolver>();
            _ = await resolver.Dispatch(command.CreateCommandId(), command, cancellationToken:cancellationToken);

            _logger.LogDebug($"Handled {command.GetType().FullName}");
        }
    }
}
