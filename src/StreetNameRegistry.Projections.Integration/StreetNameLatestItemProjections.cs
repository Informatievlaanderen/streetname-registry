namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Converters;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;

    [ConnectedProjectionName("Integratie straatnaam latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameLatestItemProjections(IOptions<IntegrationOptions> options)
        {
            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNamePersistentLocalIds = await context.StreetNameLatestItems.Where(x => x.MunicipalityId == message.Message.MunicipalityId)
                    .Select(x => x.PersistentLocalId)
                    .ToListAsync(ct);

                foreach (var persistentLocalId in streetNamePersistentLocalIds)
                {
                    await context.FindAndUpdateStreetNameLatestItem(persistentLocalId, item =>
                    {
                        item.NisCode = message.Message.NisCode;
                        item.VersionTimestamp = message.Message.Provenance.Timestamp;
                    }, ct);
                }
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var item = new StreetNameLatestItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    Status = message.Message.Status,
                    OsloStatus = message.Message.Status.Map(),
                    NisCode = message.Message.NisCode,
                    IsRemoved = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    Puri = $"{options.Value.Namespace}/{message.Message.PersistentLocalId}",
                };

                item.UpdateNameByLanguage(message.Message.Names);
                item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions);

                await context
                    .StreetNameLatestItems
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = new StreetNameLatestItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    Status = StreetNameStatus.Proposed,
                    OsloStatus = StreetNameStatus.Proposed.Map(),
                    NisCode = message.Message.NisCode,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    Puri = $"{options.Value.Namespace}/{message.Message.PersistentLocalId}",
                };

                item.UpdateNameByLanguage(message.Message.StreetNameNames);
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
                    MunicipalityId = message.Message.MunicipalityId,
                    Status = StreetNameStatus.Proposed,
                    OsloStatus = StreetNameStatus.Proposed.Map(),
                    NisCode = message.Message.NisCode,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    Puri = $"{options.Value.Namespace}/{message.Message.PersistentLocalId}",
                };

                item.UpdateNameByLanguage(message.Message.StreetNameNames);

                await context
                    .StreetNameLatestItems
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    item.OsloStatus = StreetNameStatus.Current.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    item.OsloStatus = StreetNameStatus.Proposed.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Rejected;
                    item.OsloStatus = StreetNameStatus.Rejected.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Rejected;
                    item.OsloStatus = StreetNameStatus.Rejected.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    item.OsloStatus = StreetNameStatus.Proposed.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    item.OsloStatus = StreetNameStatus.Retired.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    item.OsloStatus = StreetNameStatus.Retired.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    item.OsloStatus = StreetNameStatus.Retired.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameLatestItem(message.Message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    item.OsloStatus = StreetNameStatus.Current.Map();
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
                            case Language.Dutch:
                                item.HomonymAdditionDutch = null;
                                break;
                            case Language.French:
                                item.HomonymAdditionFrench = null;
                                break;
                            case Language.German:
                                item.HomonymAdditionGerman = null;
                                break;
                            case Language.English:
                                item.HomonymAdditionEnglish = null;
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

            When<Envelope<MunicipalityBecameCurrent>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityOfficialLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityOfficialLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Envelope<MunicipalityWasImported>>(DoNothing);
            When<Envelope<MunicipalityWasMerged>>(DoNothing);
            When<Envelope<MunicipalityWasNamed>>(DoNothing);
            When<Envelope<MunicipalityWasRetired>>(DoNothing);
            When<Envelope<MunicipalityWasRemoved>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);
        }

        private static Task DoNothing<T>(IntegrationContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
