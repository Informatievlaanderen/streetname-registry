namespace StreetNameRegistry.Projections.Legacy.StreetNameSyndication
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using global::Microsoft.EntityFrameworkCore;

    public static class StreetNameSyndicationExtensions
    {
        public static async Task CreateNewStreetNameSyndicationItem<T>(
            this LegacyContext context,
            Guid streetNameId,
            Envelope<T> message,
            Action<StreetNameSyndicationItem> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var streetNameSyndicationItem = await context.LatestPosition(streetNameId, ct);

            if (streetNameSyndicationItem == null)
                throw DatabaseItemNotFound(streetNameId);

            await CreateNewSyndicationItem(context, message, applyEventInfoOn, streetNameSyndicationItem, ct);
        }

        public static async Task CreateNewStreetNameSyndicationItem<T>(
            this LegacyContext context,
            int persistentLocalId,
            Envelope<T> message,
            Action<StreetNameSyndicationItem> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var streetNameSyndicationItem = await context.LatestPosition(persistentLocalId, ct);

            if (streetNameSyndicationItem == null)
                throw DatabaseItemNotFound(persistentLocalId);

            await CreateNewSyndicationItem(context, message, applyEventInfoOn, streetNameSyndicationItem, ct);
        }

        private static async Task CreateNewSyndicationItem<T>(LegacyContext context, Envelope<T> message,
            Action<StreetNameSyndicationItem> applyEventInfoOn, StreetNameSyndicationItem? streetNameSyndicationItem,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var provenance = message.Message.Provenance;

            var newStreetNameSyndicationItem = streetNameSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newStreetNameSyndicationItem.ApplyProvenance(provenance);
            newStreetNameSyndicationItem.SetEventData(message.Message, message.EventName);

            await context
                .StreetNameSyndication
                .AddAsync(newStreetNameSyndicationItem, ct);
        }

        public static async Task<StreetNameSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid streetNameId,
            CancellationToken ct)
            => context
                   .StreetNameSyndication
                   .Local
                   .Where(x => x.StreetNameId == streetNameId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .StreetNameSyndication
                   .Where(x => x.StreetNameId == streetNameId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static async Task<StreetNameSyndicationItem> LatestPosition(
            this LegacyContext context,
            int persistentLocalId,
            CancellationToken ct)
            => context
                   .StreetNameSyndication
                   .Local
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .StreetNameSyndication
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static void ApplyProvenance(
            this StreetNameSyndicationItem item,
            ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        public static void SetEventData<T>(this StreetNameSyndicationItem syndicationItem, T message, string eventName)
            => syndicationItem.EventDataAsXml = message.ToXml(eventName).ToString(SaveOptions.DisableFormatting);

        private static ProjectionItemNotFoundException<StreetNameSyndicationProjections> DatabaseItemNotFound(
            Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameSyndicationProjections>(streetNameId.ToString("D"));

        private static ProjectionItemNotFoundException<StreetNameSyndicationProjections> DatabaseItemNotFound(
            int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameSyndicationProjections>(
                persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
