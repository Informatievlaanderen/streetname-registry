namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using Dapper;
    using StreetName.Events;

    [ConnectedProjectionName("Integratie straatnaam versie")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameVersionProjections(
            IOptions<IntegrationOptions> options,
            ILegacyIdToPersistentLocalIdMapper legacyIdToPersistentLocalIdMapper)
        {
            #region Legacy
             When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
             {
                 var persistentLocalId = legacyIdToPersistentLocalIdMapper.Find(message.Message.StreetNameId);

                await context
                    .StreetNameVersions
                    .AddAsync(
                        new StreetNameVersion
                        {
                            PersistentLocalId = persistentLocalId,
                            StreetNameId = message.Message.StreetNameId,
                            NisCode = message.Message.NisCode,
                            Position = message.Position,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.Value.Namespace,
                            PuriId =  $"{options.Value.Namespace}/{persistentLocalId}"
                        }, ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateNameByLanguage(message.Message.Language.ToString(), message.Message.Name);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateNameByLanguage(message.Message.Language.ToString(), message.Message.Name);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateNameByLanguage(message.Message.Language.ToString(), string.Empty);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateNameByLanguage(message.Message.Language.ToString(), string.Empty);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateHomonymAdditionByLanguage(message.Message.Language.ToString(), message.Message.HomonymAddition);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateHomonymAdditionByLanguage(message.Message.Language.ToString(), message.Message.HomonymAddition);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateHomonymAdditionByLanguage(message.Message.Language.ToString(), string.Empty);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateHomonymAdditionByLanguage(message.Message.Language.ToString(), string.Empty);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    _ => {},
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    _ => {},
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    _ => {},
                    ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetName.StreetNameStatus.Current.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetName.StreetNameStatus.Proposed.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetName.StreetNameStatus.Retired.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetName.StreetNameStatus.Current.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetName.StreetNameStatus.Proposed.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.UpdateStatus(StreetNameStatus.Retired.ToString());
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.Status = null;
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.Status = null;
                    },
                    ct);
            });

            #endregion

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var item = new StreetNameVersion
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = message.Message.IsRemoved
                };

                item.Position = message.Position;
                item.UpdateStatus(message.Message.Status.ToString());

                foreach(var (language, value) in message.Message.Names)
                    item.UpdateNameByLanguage(language.ToString(), value);
                foreach(var (language, value) in message.Message.HomonymAdditions)
                    item.UpdateHomonymAdditionByLanguage(language.ToString(), value);

                await context
                    .StreetNameVersions
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var item = new StreetNameVersion
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    Namespace = options.Value.Namespace,
                    PuriId =  $"{options.Value.Namespace}/{message.Message.PersistentLocalId}"
                };

                item.Position = message.Position;
                item.UpdateStatus(StreetNameStatus.Proposed.ToString());

                foreach(var (language, value) in message.Message.StreetNameNames)
                    item.UpdateNameByLanguage(language.ToString(), value);

                await context
                    .StreetNameVersions
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Current.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Proposed.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Rejected.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Proposed.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Retired.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Retired.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.UpdateStatus(StreetNameStatus.Current.ToString());
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach(var (language, value) in message.Message.StreetNameNames)
                        item.UpdateNameByLanguage(language.ToString(), value);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach(var (language, value) in message.Message.StreetNameNames)
                        item.UpdateNameByLanguage(language.ToString(), value);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach(var (language, value) in message.Message.HomonymAdditions)
                        item.UpdateHomonymAdditionByLanguage(language.ToString(), value);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Position = message.Position;
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
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.IsRemoved = true;
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });
        }
    }

    public static class StreetNameVersionExtensions
    {
        public static async Task NewStreetNameVersion<T>(
            this IntegrationContext context,
            Guid streetNameId,
            Envelope<T> message,
            Action<StreetNameVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var r = context.StreetNameVersions.Local.FirstOrDefault(x => x.StreetNameId == streetNameId)
                ?? context.StreetNameVersions.FirstOrDefault(x => x.StreetNameId == streetNameId);

            if (r is null)
                throw DatabaseItemNotFound(streetNameId);

            var item = await context.LatestPosition(r.PersistentLocalId, ct);

            if (item == null)
                throw DatabaseItemNotFound(r.PersistentLocalId);

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


        public static void UpdateStatus(this StreetNameVersion item, string status)
        {
            string? destinationStatus = null;

            switch (status)
            {
                case "Current": item.Status = "ingebruik";
                    break;
                case "Proposed": item.Status = "voorgesteld";
                    break;
                case "Rejected": item.Status = "afgekeurd";
                    break;
                case "Retired": item.Status = "gehistoreerd";
                    break;
                default:
                    throw new InvalidOperationException($"StreetNameStatus {status} has no mapping.");
            }
        }

        public static void UpdateNameByLanguage(this StreetNameVersion item, string language, string value)
        {
            switch (language)
            {
                case "Dutch":
                    item.NameDutch = value;
                    break;
                case "French":
                    item.NameFrench = value;
                    break;
                case "German":
                    item.NameGerman = value;
                    break;
                case "English":
                    item.NameEnglish = value;
                    break;
                default: throw new InvalidOperationException($"Name language '{language}' has no mapping.");
            }
        }

        public static void UpdateHomonymAdditionByLanguage(this StreetNameVersion item, string language, string value)
        {
            switch (language)
            {
                case "Dutch":
                    item.HomonymAdditionDutch = value;
                    break;
                case "French":
                    item.HomonymAdditionFrench = value;
                    break;
                case "German":
                    item.HomonymAdditionGerman = value;
                    break;
                case "English":
                    item.HomonymAdditionEnglish = value;
                    break;
                default: throw new InvalidOperationException($"HomonymAddition language '{language}' has no mapping.");
            }
        }

        private static ProjectionItemNotFoundException<StreetNameVersionProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameVersionProjections>(persistentLocalId.ToString(CultureInfo.InvariantCulture));

        private static ProjectionItemNotFoundException<StreetNameVersionProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<StreetNameVersionProjections>(streetNameId.ToString("D"));
    }
}
