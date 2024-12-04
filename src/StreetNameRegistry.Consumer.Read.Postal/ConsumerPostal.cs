namespace StreetNameRegistry.Consumer.Read.Postal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class ConsumerPostal : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<ConsumerPostalContext> _consumerPostalDbContextFactory;
        private readonly IConsumer _consumer;
        private readonly ILogger<ConsumerPostal> _logger;

        public ConsumerPostal(
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<ConsumerPostalContext> consumerPostalDbContextFactory,
            IConsumer consumer,
            ILoggerFactory loggerFactory)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _consumerPostalDbContextFactory = consumerPostalDbContextFactory;
            _consumer = consumer;
            _logger = loggerFactory.CreateLogger<ConsumerPostal>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var projector =
                new ConnectedProjector<ConsumerPostalContext>(
                    Resolve.WhenEqualToHandlerMessageType(new PostalKafkaProjection().Handlers));

            try
            {
                await _consumer.ConsumeContinuously(async (message, messageContext) => { await ConsumeHandler(projector, message, messageContext); }, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(ConsumerPostal)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task ConsumeHandler(
            ConnectedProjector<ConsumerPostalContext> projector,
            object message,
            MessageContext messageContext)
        {
            _logger.LogInformation("Handling next message");

            await using var context = await _consumerPostalDbContextFactory.CreateDbContextAsync(CancellationToken.None);
            await projector.ProjectAsync(context, message, CancellationToken.None).ConfigureAwait(false);

            await context.UpdateProjectionState(typeof(ConsumerPostal).FullName, messageContext.Offset, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
