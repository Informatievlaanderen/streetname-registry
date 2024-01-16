namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    using Municipality.Events;
    using Municipality;
    using StreetName.Events;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    [ConnectedProjectionName("Integratie straatnaam versie")]
    [ConnectedProjectionDescription("Projectie die de laatste straatnaam data voor de integratie database bijhoudt.")]
    public class StreetNameVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public StreetNameVersionProjections(
            IOptions<IntegrationOptions> options,
            IEventsRepository eventsRepository)
        {
            #region Legacy

            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                var persistentLocalId = await eventsRepository.GetPersistentLocalId(message.Message.StreetNameId);

                if (!persistentLocalId.HasValue)
                {
                    throw new InvalidOperationException($"No persistent local id found for {message.Message.StreetNameId}");
                }

                await context
                    .StreetNameVersions
                    .AddAsync(
                        new StreetNameVersion
                        {
                            PersistentLocalId = persistentLocalId.Value,
                            StreetNameId = message.Message.StreetNameId,
                            MunicipalityId = message.Message.MunicipalityId,
                            NisCode = message.Message.NisCode,
                            Position = message.Position,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Namespace = options.Value.Namespace,
                            Puri = $"{options.Value.Namespace}/{persistentLocalId}"
                        }, ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateNameByLanguage(message.Message.Language, message.Message.Name); },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateNameByLanguage(message.Message.Language, message.Message.Name); },
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateNameByLanguage(message.Message.Language, string.Empty); },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateNameByLanguage(message.Message.Language, string.Empty); },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateHomonymAdditionByLanguage(message.Message.Language, message.Message.HomonymAddition); },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateHomonymAdditionByLanguage(message.Message.Language, message.Message.HomonymAddition); },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateHomonymAdditionByLanguage(message.Message.Language, string.Empty); },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.UpdateHomonymAdditionByLanguage(message.Message.Language, string.Empty); },
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    item =>
                    {
                        item.IsRemoved = true;
                    },
                    ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity => { entity.PersistentLocalId = message.Message.PersistentLocalId; },
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(
                    message.Message.StreetNameId,
                    message,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Current;
                        entity.OsloStatus = StreetNameStatus.Current.Map();
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
                        entity.Status = StreetNameStatus.Proposed;
                        entity.OsloStatus = StreetNameStatus.Proposed.Map();
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
                        entity.Status = StreetNameStatus.Retired;
                        entity.OsloStatus = StreetNameStatus.Retired.Map();
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
                        entity.Status = StreetNameStatus.Current;
                        entity.OsloStatus = StreetNameStatus.Current.Map();
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
                        entity.Status = StreetNameStatus.Proposed;
                        entity.OsloStatus = StreetNameStatus.Proposed.Map();
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
                        entity.Status = StreetNameStatus.Retired;
                        entity.OsloStatus = StreetNameStatus.Retired.Map();
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
                        entity.OsloStatus = null;
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
                        entity.OsloStatus = null;
                    },
                    ct);
            });

            #endregion

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNamePersistentLocalIds = await context.StreetNameVersions.Where(x => x.MunicipalityId == message.Message.MunicipalityId)
                    .Select(x => x.PersistentLocalId)
                    .ToListAsync(ct);

                foreach (var persistentLocalId in streetNamePersistentLocalIds)
                {
                    await context.NewStreetNameVersion(persistentLocalId, message, item =>
                    {
                        item.NisCode = message.Message.NisCode;
                        item.VersionTimestamp = message.Message.Provenance.Timestamp;
                    }, ct);
                }
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var item = new StreetNameVersion
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = message.Message.IsRemoved,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    Puri = $"{options.Value.Namespace}/{message.Message.PersistentLocalId}"
                };

                item.Position = message.Position;
                item.Status = message.Message.Status;
                item.OsloStatus = message.Message.Status.Map();

                foreach (var (language, value) in message.Message.Names)
                    item.UpdateNameByLanguage(language, value);
                foreach (var (language, value) in message.Message.HomonymAdditions)
                    item.UpdateHomonymAdditionByLanguage(language, value);

                await context
                    .StreetNameVersions
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var item = new StreetNameVersion
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    Puri = $"{options.Value.Namespace}/{message.Message.PersistentLocalId}"
                };

                item.Position = message.Position;
                item.Status = StreetNameStatus.Proposed;
                item.OsloStatus = StreetNameStatus.Proposed.Map();

                foreach (var (language, value) in message.Message.StreetNameNames)
                    item.UpdateNameByLanguage(language, value);

                await context
                    .StreetNameVersions
                    .AddAsync(item, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    item.OsloStatus = StreetNameStatus.Current.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    item.OsloStatus = StreetNameStatus.Proposed.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Rejected;
                    item.OsloStatus = StreetNameStatus.Rejected.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    item.OsloStatus = StreetNameStatus.Proposed.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    item.OsloStatus = StreetNameStatus.Retired.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    item.OsloStatus = StreetNameStatus.Retired.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    item.OsloStatus = StreetNameStatus.Current.Map();
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach (var (language, value) in message.Message.StreetNameNames)
                        item.UpdateNameByLanguage(language, value);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach (var (language, value) in message.Message.StreetNameNames)
                        item.UpdateNameByLanguage(language, value);
                    item.VersionTimestamp = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.NewStreetNameVersion(message.Message.PersistentLocalId, message, item =>
                {
                    foreach (var (language, value) in message.Message.HomonymAdditions)
                        item.UpdateHomonymAdditionByLanguage(language, value);
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
}
