namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameNameV2
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameNameExtensionsV2
    {
        public static async Task<StreetNameNameV2> FindAndUpdateStreetNameName(
            this LegacyContext context,
            int persistentLocalId,
            Action<StreetNameNameV2> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameNamesV2
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameNameProjectionsV2> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameNameProjectionsV2>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
