namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameList
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameListExtensions
    {
        public static async Task<StreetNameListItem> FindAndUpdateStreetNameListItem(
            this LegacyContext context,
            Guid streetNameId,
            Action<StreetNameListItem> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameList
                .FindAsync(streetNameId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(streetNameId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameListProjections> DatabaseItemNotFound(Guid streetName)
            => new ProjectionItemNotFoundException<StreetNameListProjections>(streetName.ToString("D"));
    }
}
