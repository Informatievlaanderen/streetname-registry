namespace StreetNameRegistry.Projections.Wms.StreetNameHelperV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using NodaTime;

    [ConnectedProjectionName("WMS adressen")]
    [ConnectedProjectionDescription("Projectie die de straatnaam data voor het WMS adressenregister voorziet.")]
    public sealed class StreetNameHelperV2Projections: ConnectedProjection<WmsContext>
    {
        public StreetNameHelperV2Projections()
        {
            When<Envelope<MunicipalityNisCodeWasChanged>>((context, message, ct) =>
            {
                var streetNames = context
                    .StreetNameHelperV2
                    .Local
                    .Where(s => s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNameHelperV2.Where(s => s.MunicipalityId == message.Message.MunicipalityId));

                foreach (var streetName in streetNames)
                    streetName.NisCode = message.Message.NisCode;

                return Task.CompletedTask;
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var entity = new StreetNameHelperV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    Removed = message.Message.IsRemoved,
                    Status = message.Message.Status,
                    Version = message.Message.Provenance.Timestamp,
                };
                UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                UpdateNameByLanguage(entity, message.Message.Names);
                UpdateHomonymAdditionByLanguage(entity, new HomonymAdditions(message.Message.HomonymAdditions));
                await context
                    .StreetNameHelperV2
                    .AddAsync(entity, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var entity = new StreetNameHelperV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    Removed = false,
                    Status = StreetNameStatus.Proposed,
                    Version = message.Message.Provenance.Timestamp,
                };
                UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                UpdateNameByLanguage(entity, message.Message.StreetNameNames);
                await context
                    .StreetNameHelperV2
                    .AddAsync(entity, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Rejected);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });


            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateStatus(streetNameHelperV2, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateNameByLanguage(streetNameHelperV2, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateNameByLanguage(streetNameHelperV2, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    UpdateHomonymAdditionByLanguage(streetNameHelperV2, new HomonymAdditions(message.Message.HomonymAdditions));
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case Language.Dutch: streetNameHelperV2.HomonymAdditionDutch = null;
                                break;
                            case Language.French: streetNameHelperV2.HomonymAdditionFrench = null;
                                break;
                            case Language.German: streetNameHelperV2.HomonymAdditionGerman = null;
                                break;
                            case Language.English: streetNameHelperV2.HomonymAdditionEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });


            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameHelper(message.Message.PersistentLocalId, streetNameHelperV2 =>
                {
                    streetNameHelperV2.Removed = true;
                    UpdateVersionTimestamp(streetNameHelperV2, message.Message.Provenance.Timestamp);
                }, ct);
            });
        }

        private static void UpdateNameByLanguage(StreetNameHelperV2 entity, IDictionary<Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.NameDutch = streetNameName;
                        break;

                    case Language.French:
                        entity.NameFrench = streetNameName;
                        break;

                    case Language.German:
                        entity.NameGerman = streetNameName;
                        break;

                    case Language.English:
                        entity.NameEnglish = streetNameName;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), streetNameName, null);
                }
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameHelperV2 entity, List<StreetNameHomonymAddition> homonymAdditions)
        {
            foreach (var homonymAddition in homonymAdditions)
            {
                switch (homonymAddition.Language)
                {
                    case Language.Dutch:
                        entity.HomonymAdditionDutch = homonymAddition.HomonymAddition;
                        break;

                    case Language.French:
                        entity.HomonymAdditionFrench = homonymAddition.HomonymAddition;
                        break;

                    case Language.German:
                        entity.HomonymAdditionGerman =  homonymAddition.HomonymAddition;
                        break;

                    case Language.English:
                        entity.HomonymAdditionEnglish =  homonymAddition.HomonymAddition;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(homonymAddition.Language), homonymAddition.Language, null);
                }
            }
        }

        private static void UpdateVersionTimestamp(StreetNameHelperV2 streetName, Instant versionTimestamp)
            => streetName.Version = versionTimestamp;

        private static void UpdateStatus(StreetNameHelperV2 streetName, StreetNameStatus status)
            => streetName.Status = status;
    }
}
