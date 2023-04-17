namespace StreetNameRegistry.Projections.BackOffice
{
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Municipality.Events;

    public sealed class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<StreetNameWasProposedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentMunicipalityStreetNameIdRelation(message.Message.PersistentLocalId, message.Message.MunicipalityId, message.Message.NisCode, cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });
        }
    }
}
