namespace StreetNameRegistry.Projections.BackOffice.Microsoft
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using global::Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using Municipality.Events;

    public sealed class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<StreetNameWasProposedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentMunicipalityStreetNameIdRelation(message.Message.PersistentLocalId, message.Message.MunicipalityId, cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });
        }
    }
}
