namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using global::Microsoft.EntityFrameworkCore;

    public static class StreetNameExtractExtensionsV2
    {
        public static async Task<StreetNameExtractItemV2> FindAndUpdateStreetNameExtract(
            this ExtractContext context,
            int persistentLocalId,
            Action<StreetNameExtractItemV2> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameExtractV2
                .SingleOrDefaultAsync(x => x.StreetNamePersistentLocalId == persistentLocalId, cancellationToken: ct);

            if (streetName == null)
            {
                throw DatabaseItemNotFound(persistentLocalId);
            }

            updateFunc(streetName);

            return streetName;
        }

        private static ProjectionItemNotFoundException<StreetNameExtractProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameExtractProjections>(persistentLocalId.ToString());
    }
}
