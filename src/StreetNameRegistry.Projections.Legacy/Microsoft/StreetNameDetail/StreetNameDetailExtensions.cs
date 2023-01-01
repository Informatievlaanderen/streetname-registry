namespace StreetNameRegistry.Projections.Legacy.Microsoft.StreetNameDetail
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameDetailExtensions
    {
        public static async Task<StreetNameDetail> FindAndUpdateStreetNameDetail(
            this LegacyContext context,
            Guid streetNameId,
            Action<StreetNameDetail> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameDetail
                .FindAsync(streetNameId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(streetNameId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameDetailProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameDetailProjections>(streetNameId.ToString("D"));
    }
}
