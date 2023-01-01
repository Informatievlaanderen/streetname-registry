namespace StreetNameRegistry.Projections.Extract.Microsoft.StreetNameExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class StreetNameExtractExtensions
    {
        public static async Task<StreetNameExtractItem> FindAndUpdateStreetNameExtract(
            this ExtractContext context,
            Guid streetNameId,
            Action<StreetNameExtractItem> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameExtract
                .FindAsync(streetNameId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(streetNameId);

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameExtractProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameExtractProjections>(streetNameId.ToString("D"));
    }
}
