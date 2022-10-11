namespace StreetNameRegistry.Projections.Legacy.StreetNameSyndication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using StreetName.Events;
    using StreetName.Events.Crab;
    using LanguageHelpers = StreetName.LanguageHelpers;
    using StreetNameName = Municipality.StreetNameName;

    [ConnectedProjectionName("Feed endpoint straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor de straatnamen feed voorziet.")]
    public sealed class StreetNameSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public StreetNameSyndicationProjections()
        {
            #region Legacy Events

            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                var streetNameSyndicationItem = new StreetNameSyndicationItem
                {
                    Position = message.Position,
                    StreetNameId = message.Message.StreetNameId,
                    NisCode = message.Message.NisCode,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                streetNameSyndicationItem.ApplyProvenance(message.Message.Provenance);
                streetNameSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .StreetNameSyndication
                    .AddAsync(streetNameSyndicationItem, ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.PersistentLocalId = message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateNameByLanguage(x, message.Message.Name, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateNameByLanguage(x, null, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateNameByLanguage(x, message.Message.Name, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateNameByLanguage(x, null, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateHomonymAdditionByLanguage(x, message.Message.HomonymAddition, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateHomonymAdditionByLanguage(x, null, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateHomonymAdditionByLanguage(x, message.Message.HomonymAddition, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => UpdateHomonymAdditionByLanguage(x, null, LanguageHelpers.ToNullableMunicipalityLanguage(message.Message.Language)),
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Current,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Current,
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Proposed,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Proposed,
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Retired,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = StreetNameStatus.Retired,
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.IsComplete = true,
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => x.IsComplete = false,
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(
                    message.Message.StreetNameId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, message, ct) => await DoNothing());

            When<Envelope<StreetNameWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());

            #endregion

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var streetNameSyndicationItem = new StreetNameSyndicationItem
                {
                    Position = message.Position,
                    StreetNameId = null, //while we have the information, we shouldn't identify streetname with his old internal id
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow,
                    Status = message.Message.Status,
                    IsComplete = true
                };
                UpdateNameByLanguage(streetNameSyndicationItem, new Names(message.Message.Names));
                UpdateHomonymAdditionByLanguage(streetNameSyndicationItem, new HomonymAdditions(message.Message.HomonymAdditions));
                streetNameSyndicationItem.ApplyProvenance(message.Message.Provenance);
                streetNameSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .StreetNameSyndication
                    .AddAsync(streetNameSyndicationItem, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var streetNameSyndicationItem = new StreetNameSyndicationItem
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow,
                    Status = StreetNameStatus.Proposed,
                    IsComplete = true
                };
                UpdateNameByLanguage(streetNameSyndicationItem, message.Message.StreetNameNames);
                streetNameSyndicationItem.ApplyProvenance(message.Message.Provenance);
                streetNameSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .StreetNameSyndication
                    .AddAsync(streetNameSyndicationItem, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(message.Message.PersistentLocalId, message, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Current);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(message.Message.PersistentLocalId, message, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Proposed);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(message.Message.PersistentLocalId, message, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Rejected);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(message.Message.PersistentLocalId, message, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Retired);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameSyndicationItem(message.Message.PersistentLocalId, message, streetNameNameV2 =>
                {
                    UpdateNameByLanguage(streetNameNameV2, message.Message.StreetNameNames);
                }, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var persistentLocalIds = context
                    .StreetNameSyndication
                    .Local
                    .Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId
                        && s.PersistentLocalId.HasValue)
                    .Union(context.StreetNameSyndication.Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId
                        && s.PersistentLocalId.HasValue))
                    .Select(s => s.PersistentLocalId);

                foreach (var persistentLocalId in persistentLocalIds)
                {
                    await context.CreateNewStreetNameSyndicationItem(persistentLocalId!.Value, message, syndicationItem =>
                    {
                        syndicationItem.NisCode = message.Message.NisCode;
                    }, ct);
                }
            });
        }

        private static void UpdateNameByLanguage(StreetNameSyndicationItem streetNameSyndicationItem, string? name, Language? language)
        {
            switch (language)
            {
                case Language.Dutch:
                    streetNameSyndicationItem.NameDutch = name;
                    break;
                case Language.French:
                    streetNameSyndicationItem.NameFrench = name;
                    break;
                case Language.German:
                    streetNameSyndicationItem.NameGerman = name;
                    break;
                case Language.English:
                    streetNameSyndicationItem.NameEnglish = name;
                    break;
            }
        }

        private static void UpdateNameByLanguage(StreetNameSyndicationItem streetNameSyndicationItem, List<StreetNameName> streetNameNames)
        {
            foreach (var streetNameName in streetNameNames)
            {
                UpdateNameByLanguage(streetNameSyndicationItem, streetNameName.Name, streetNameName.Language);
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameSyndicationItem streetNameSyndicationItem, List<StreetNameHomonymAddition> homonymAdditions)
        {
            foreach (var homonymAddition in homonymAdditions)
            {
                UpdateHomonymAdditionByLanguage(streetNameSyndicationItem, homonymAddition.HomonymAddition, homonymAddition.Language);
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameSyndicationItem streetNameSyndicationItem, string? homonymAddition, Language? language)
        {
            switch (language)
            {
                case Language.Dutch:
                    streetNameSyndicationItem.HomonymAdditionDutch = homonymAddition;
                    break;
                case Language.French:
                    streetNameSyndicationItem.HomonymAdditionFrench = homonymAddition;
                    break;
                case Language.German:
                    streetNameSyndicationItem.HomonymAdditionGerman = homonymAddition;
                    break;
                case Language.English:
                    streetNameSyndicationItem.HomonymAdditionEnglish = homonymAddition;
                    break;
            }
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }

        private static void UpdateStatus(StreetNameSyndicationItem syndicationItem, StreetNameStatus status)
            => syndicationItem.Status = status;
    }
}
