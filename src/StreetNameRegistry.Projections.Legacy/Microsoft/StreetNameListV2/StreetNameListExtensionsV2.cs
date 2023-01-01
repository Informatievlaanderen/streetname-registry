namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameListV2
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameListExtensionsV2
    {
        public static async Task<StreetNameListItemV2> FindAndUpdateStreetNameListItem(
            this LegacyContext context,
            int persistentLocalId,
            Action<StreetNameListItemV2> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameListV2
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameListProjectionsV2> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameListProjectionsV2>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
