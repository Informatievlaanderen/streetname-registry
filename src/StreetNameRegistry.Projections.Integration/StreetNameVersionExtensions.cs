namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class StreetNameVersionExtensions
    {
        public static async Task NewStreetNameVersion<T>(
            this IntegrationContext context,
            Guid streetNameId,
            Envelope<T> message,
            Action<StreetNameVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var item = await context.LatestPosition(streetNameId, ct);

            if (item == null)
                throw DatabaseItemNotFound(streetNameId);

            var version = item.CloneAndApplyEventInfo(
                message.Position,
                message.Message.Provenance.Timestamp,
                applyEventInfoOn);

            await context
                .StreetNameVersions
                .AddAsync(version, ct);
        }

        public static async Task NewStreetNameVersion<T>(
            this IntegrationContext context,
            int persistentLocalId,
            Envelope<T> message,
            Action<StreetNameVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var item = await context.LatestPosition(persistentLocalId, ct);

            if (item == null)
                throw DatabaseItemNotFound(persistentLocalId);

            var version = item.CloneAndApplyEventInfo(
                message.Position,
                message.Message.Provenance.Timestamp,
                applyEventInfoOn);

            await context
                .StreetNameVersions
                .AddAsync(version, ct);
        }

        private static async Task<StreetNameVersion> LatestPosition(
            this IntegrationContext context,
            int persistentLocalId,
            CancellationToken ct)
            => context
                   .StreetNameVersions
                   .Local
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .StreetNameVersions
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static async Task<StreetNameVersion> LatestPosition(
            this IntegrationContext context,
            Guid streetNameId,
            CancellationToken ct)
            => context
                   .StreetNameVersions
                   .Local
                   .Where(x => x.StreetNameId == streetNameId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .StreetNameVersions
                   .Where(x => x.StreetNameId == streetNameId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<StreetNameVersionProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameVersionProjections>(persistentLocalId.ToString(CultureInfo.InvariantCulture));

        private static ProjectionItemNotFoundException<StreetNameVersionProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameVersionProjections>(streetNameId.ToString("D"));
    }
}
