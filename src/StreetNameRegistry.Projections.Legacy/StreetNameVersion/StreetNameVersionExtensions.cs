namespace StreetNameRegistry.Projections.Legacy.StreetNameVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class StreetNameVersionExtensions
    {
        public static async Task CreateNewStreetNameVersionItem<T>(
            this LegacyContext context,
            Guid streetNameId,
            Envelope<T> message,
            Action<StreetNameVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance
        {
            var streetNameVersion = await context.LatestPosition(streetNameId, ct);

            if (streetNameVersion == null)
                throw DatabaseItemNotFound(streetNameId);

            var provenance = message.Message.Provenance;

            var newStreetNameVersion = streetNameVersion.CloneAndApplyEventInfo(
                message.Position,
                applyEventInfoOn);

            newStreetNameVersion.ApplyProvenance(provenance);

            await context
                .StreetNameVersions
                .AddAsync(newStreetNameVersion, ct);
        }

        public static async Task<StreetNameVersion> LatestPosition(
            this LegacyContext context,
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

        public static async Task<IEnumerable<StreetNameVersion>> AllVersions(
            this LegacyContext context,
            Guid streetNameId,
            CancellationToken cancellationToken)
        {
            var sqlEntities = await context
                .StreetNameVersions
                .Where(x => x.StreetNameId == streetNameId)
                .ToListAsync(cancellationToken);

            var localEntities = context
                .StreetNameVersions
                .Local
                .Where(x => x.StreetNameId == streetNameId)
                .ToList();

            return sqlEntities
                .Union(localEntities)
                .Distinct();
        }

        public static void ApplyProvenance(
            this StreetNameVersion streetNameVersion,
            ProvenanceData provenance)
        {
            streetNameVersion.Organisation = provenance.Organisation;
            streetNameVersion.Application = provenance.Application;
            streetNameVersion.Reason = provenance.Reason;
            streetNameVersion.Modification = provenance.Modification;
            streetNameVersion.Operator = provenance.Operator;
            streetNameVersion.VersionTimestamp = provenance.Timestamp;
        }

        private static ProjectionItemNotFoundException<StreetNameVersionProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameVersionProjections>(streetNameId.ToString("D"));
    }
}
