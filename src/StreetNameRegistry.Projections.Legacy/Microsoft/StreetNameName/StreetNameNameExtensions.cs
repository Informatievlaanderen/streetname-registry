namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameNameExtensions
    {
        public static async Task<StreetNameName> FindAndUpdateStreetNameName(
            this LegacyContext context,
            Guid streetNameId,
            Action<StreetNameName> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameNames
                .FindAsync(streetNameId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(streetNameId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameNameProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameNameProjections>(streetNameId.ToString("D"));
    }
}
