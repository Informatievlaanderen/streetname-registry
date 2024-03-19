namespace StreetNameRegistry.Projector.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Consumer;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using StreetNameRegistry.Infrastructure;

    [ApiVersion("1.0")]
    [ApiRoute("consumers")]
    public class ConsumersController : ApiController
    {
        private const string? ConsumerConnectionStringKey = "Consumer";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            await using var sqlConnection = new SqlConnection(configuration.GetConnectionString(ConsumerConnectionStringKey));
            var result =
                await sqlConnection.QueryFirstAsync<DateTimeOffset>(
                    $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{Schema.Consumer}].[{IdempotentConsumerContext.ProcessedMessageTable}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC");

            return Ok(new []
            {
                new
                {
                    Name = "Consumer van gemeente",
                    LastProcessedMessage = result
                }
            });
        }
    }
}
