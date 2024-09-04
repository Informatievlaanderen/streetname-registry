namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;

    public interface IScopedIdempotentCommandHandler: IIdempotentCommandHandler
    {
    }

    public class ScopedIdempotentCommandHandler : IScopedIdempotentCommandHandler
    {
        private readonly ILifetimeScope _container;

        public ScopedIdempotentCommandHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task<long> Dispatch(Guid? commandId, object command, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            await using var scope = _container.BeginLifetimeScope();

            var resolver = scope.Resolve<IIdempotentCommandHandler>();
            return await resolver.Dispatch(
                commandId,
                command,
                metadata,
                cancellationToken);
        }
    }
}
