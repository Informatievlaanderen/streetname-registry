namespace StreetNameRegistry.Consumer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public sealed class Consumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Consumer> _logger;
        private readonly IIdempotentConsumer<IdempotentConsumerContext> _kafkaIdemIdompotencyConsumer;
        private readonly IDbContextFactory<ConsumerContext> _consumerContextFactory;

        public Consumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<IdempotentConsumerContext> kafkaIdemIdompotencyConsumer,
            IDbContextFactory<ConsumerContext> consumerContextFactory)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdompotencyConsumer = kafkaIdemIdompotencyConsumer;
            _consumerContextFactory = consumerContextFactory;

            _logger = loggerFactory.CreateLogger<Consumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);
            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(new MunicipalityKafkaProjection(_consumerContextFactory).Handlers));

            try
            {
                await _kafkaIdemIdompotencyConsumer.ConsumeContinuously(async (message, consumerContext) =>
                {
                    await ConsumeHandler(commandHandlingProjector, commandHandler, message, consumerContext);
                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task ConsumeHandler(
            ConnectedProjector<CommandHandler> commandHandlingProjector,
            CommandHandler commandHandler,
            object message,
            IdempotentConsumerContext consumerContext)
        {
            _logger.LogInformation("Handling next message");

            await commandHandlingProjector.ProjectAsync(commandHandler, message, CancellationToken.None).ConfigureAwait(false);

            await consumerContext.SaveChangesAsync(CancellationToken.None);
        }
    }
}
