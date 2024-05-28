namespace StreetNameRegistry.Projections.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Municipality.Events;

    public sealed class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory, IConfiguration configuration)
        {
            var delayInSeconds = configuration.GetValue("DelayInSeconds", 10);

            When<Envelope<StreetNameWasProposedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentMunicipalityStreetNameIdRelation(message.Message.PersistentLocalId, message.Message.MunicipalityId, message.Message.NisCode, cancellationToken);
            });
        }

        private static async Task DelayProjection<TMessage>(Envelope<TMessage> envelope, int delayInSeconds, CancellationToken cancellationToken)
            where TMessage : IMessage
        {
            var differenceInSeconds = (DateTime.UtcNow - envelope.CreatedUtc).TotalSeconds;
            if (differenceInSeconds < delayInSeconds)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds - differenceInSeconds), cancellationToken);
            }
        }
    }
}
