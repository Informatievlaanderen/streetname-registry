namespace StreetNameRegistry.Projections.Wfs.Microsoft.StreetNameHelper
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameHelperExtensions
    {
        public static async Task<StreetNameHelper> FindAndUpdateStreetNameHelper(
            this WfsContext context,
            Guid streetNameId,
            Action<StreetNameHelper> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameHelper
                .FindAsync(streetNameId, cancellationToken: ct);

            if (streetName == null)
            {
                throw DatabaseItemNotFound(streetNameId);
            }

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameHelperProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameHelperProjections>(streetNameId.ToString("D"));
    }
}
