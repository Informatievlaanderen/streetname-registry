namespace StreetNameRegistry.Consumer.Read.Postal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
                await _consumer.ConsumeContinuously(async message =>
                {
                    _logger.LogInformation("Handling next message");

                    await using var context = await _consumerPostalDbContextFactory.CreateDbContextAsync(stoppingToken);
                    await projector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);

                }, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(ConsumerPostal)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
