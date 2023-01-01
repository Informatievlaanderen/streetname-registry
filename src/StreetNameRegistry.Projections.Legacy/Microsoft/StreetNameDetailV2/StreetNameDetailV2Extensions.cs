namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameDetailV2
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameDetailV2Extensions
    {
        public static async Task<StreetNameDetailV2> FindAndUpdateStreetNameDetailV2(
            this LegacyContext context,
            int persistentLocalId,
            Action<StreetNameDetailV2> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameDetailV2
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameDetailProjectionsV2> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameDetailProjectionsV2>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
