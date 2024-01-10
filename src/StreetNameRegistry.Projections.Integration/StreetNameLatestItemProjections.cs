namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;

    [ConnectedProjectionName("Integratie straatnaam latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameLatestItemProjections(IOptions<IntegrationOptions> options)
        {
            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var item = new StreetNameLatestItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = message.Message.IsRemoved
                };

                item.UpdateStatus( message.Message.Status);
                item.UpdateNameByLanguage(message.Message.Names);
                item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions);

                await context
                    .StreetNameLatestItems
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var item = new StreetNameLatestItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = false
                };

                item.UpdateStatus( StreetNameStatus.Proposed);
                item.UpdateNameByLanguage(message.Message.StreetNameNames);

                await context
                    .StreetNameLatestItems
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Current);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Proposed);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Rejected);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Proposed);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Retired);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Retired);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateStatus( StreetNameStatus.Current);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateNameByLanguage(message.Message.StreetNameNames);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateNameByLanguage(message.Message.StreetNameNames);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case Language.Dutch: item.HomonymAdditionDutch = null;
                                break;
                            case Language.French: item.HomonymAdditionFrench = null;
                                break;
                            case Language.German: item.HomonymAdditionGerman = null;
                                break;
                            case Language.English: item.HomonymAdditionEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.IsRemoved = true;
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });
        }
    }

    public static class StreetNameLatestItemExtensions
    {
        public static async Task FindAndUpdateStreetNameLatestItem(
            this IntegrationContext context,
            int persistentLocalId,
            Action<StreetNameLatestItem> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameLatestItems
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);
        }

        public static void UpdateStatus(this StreetNameLatestItem item, StreetNameStatus sourceStatus)
        {
            string? destinationStatus = null;

            if (sourceStatus is StreetNameStatus.Current)
                destinationStatus = "ingebruik";
            else if (sourceStatus is StreetNameStatus.Proposed)
                destinationStatus = "voorgesteld";
            else if (sourceStatus is StreetNameStatus.Rejected)
                destinationStatus = "afgekeurd";
            else if (sourceStatus is StreetNameStatus.Retired)
                destinationStatus = "gehistoreerd";

            item.Status = destinationStatus ?? throw new InvalidOperationException($"StreetNameStatus {sourceStatus} has no mapping.");
        }

        public static void UpdateNameByLanguage(this StreetNameLatestItem item, dynamic streetNameNames)
        {
            item.NameDutch = streetNameNames[Language.Dutch];
            item.NameGerman = streetNameNames[Language.German];
            item.NameFrench = streetNameNames[Language.French];
            item.NameEnglish = streetNameNames[Language.English];
        }

        public static void UpdateHomonymAdditionByLanguage(this StreetNameLatestItem item, IDictionary<Language, string> homonymAdditions)
        {
            item.HomonymAdditionDutch = homonymAdditions[Language.Dutch];
            item.HomonymAdditionGerman = homonymAdditions[Language.German];
            item.HomonymAdditionFrench = homonymAdditions[Language.French];
            item.HomonymAdditionEnglish = homonymAdditions[Language.English];
        }

        private static ProjectionItemNotFoundException<StreetNameLatestItemProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameLatestItemProjections>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
