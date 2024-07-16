namespace StreetNameRegistry.Projections.Integration.Merger
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality.Events;

    [ConnectedProjectionName("Integratie straatnaam fusie item")]
    [ConnectedProjectionDescription("Projectie die de fusie straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameMergerItemProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameMergerItemProjections()
        {
            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                foreach (var mergedStreetNamePersistentLocalId in message.Message.MergedStreetNamePersistentLocalIds)
                {
                    var item = new StreetNameMergerItem(message.Message.PersistentLocalId, mergedStreetNamePersistentLocalId);

                    await context
                        .StreetNameMergerItems
                        .AddAsync(item, ct);
                }
            });
        }
    }
}
