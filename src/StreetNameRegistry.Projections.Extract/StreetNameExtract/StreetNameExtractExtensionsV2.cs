namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StreetNameExtractExtensionsV2
    {
        public static async Task<StreetNameExtractItemV2> FindAndUpdateStreetNameExtract(
            this ExtractContext context,
            Guid municipalityId,
            int persistentLocalId,
            Action<StreetNameExtractItemV2> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameExtractV2
                .FindAsync(new object[] { municipalityId, persistentLocalId }, ct);

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
