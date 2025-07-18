namespace StreetNameRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using Newtonsoft.Json;
    using NodaTime;

    [ConnectedProjectionName("Kafka producer ldes")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string StreetNameTopicKey = "StreetNameTopic";

        private readonly IProducer _producer;
        private readonly string _osloNamespace;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ProducerProjections(
            IProducer producer,
            string osloNamespace,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _producer = producer;
            _osloNamespace = osloNamespace;
            _jsonSerializerSettings = jsonSerializerSettings;

            When<Envelope<StreetNameOsloSnapshotsWereRequested>>(async (context, message, ct) =>
            {
                foreach (var persistentLocalId in message.Message.PersistentLocalIds)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await Produce(context, persistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var streetNameDetail = new StreetNameDetail
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = message.Message.IsRemoved,
                    Status = message.Message.Status
                };

                UpdateNameByLanguage(streetNameDetail, message.Message.Names);
                UpdateHomonymAdditionByLanguage(streetNameDetail, new HomonymAdditions(message.Message.HomonymAdditions));

                await context
                    .StreetNames
                    .AddAsync(streetNameDetail, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var streetNameDetail = new StreetNameDetail
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameDetail, message.Message.StreetNameNames);
                UpdateHomonymAdditionByLanguage(streetNameDetail, new HomonymAdditions(message.Message.HomonymAdditions));

                await context
                    .StreetNames
                    .AddAsync(streetNameDetail, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var streetNameDetail = new StreetNameDetail
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameDetail, message.Message.StreetNameNames);

                await context
                    .StreetNames
                    .AddAsync(streetNameDetail, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Rejected);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Rejected);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateStatus(streetNameDetail, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateNameByLanguage(streetNameDetail, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateNameByLanguage(streetNameDetail, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    UpdateHomonymAdditionByLanguage(streetNameDetail, new HomonymAdditions(message.Message.HomonymAdditions));
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case Language.Dutch:
                                streetNameDetail.HomonymAdditionDutch = null;
                                break;
                            case Language.French:
                                streetNameDetail.HomonymAdditionFrench = null;
                                break;
                            case Language.German:
                                streetNameDetail.HomonymAdditionGerman = null;
                                break;
                            case Language.English:
                                streetNameDetail.HomonymAdditionEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(language), language, null);
                        }
                    }

                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetail(message.Message.PersistentLocalId, streetNameDetail =>
                {
                    streetNameDetail.IsRemoved = true;
                    UpdateVersionTimestamp(streetNameDetail, message.Message.Provenance.Timestamp);
                }, ct);

                await Produce(context, message.Message.PersistentLocalId, message.Position, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNamesToChange = context
                    .StreetNames
                    .Local
                    .Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNames.Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId));

                foreach (var streetNameDetail in streetNamesToChange)
                {
                    streetNameDetail.NisCode = message.Message.NisCode;
                    await Produce(context, streetNameDetail.StreetNamePersistentLocalId, message.Position, ct);
                }
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
        }

        private static void UpdateNameByLanguage(StreetNameDetail entity, IDictionary<Language, string> streetNameNames)
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
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameDetail entity, List<StreetNameHomonymAddition> homonymAdditions)
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
                        entity.HomonymAdditionGerman = homonymAddition.HomonymAddition;
                        break;

                    case Language.English:
                        entity.HomonymAdditionEnglish = homonymAddition.HomonymAddition;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(homonymAddition.Language), homonymAddition.Language, null);
                }
            }
        }

        private static void UpdateVersionTimestamp(StreetNameDetail streetName, Instant versionTimestamp)
            => streetName.VersionTimestamp = versionTimestamp;

        private static void UpdateStatus(StreetNameDetail streetName, StreetNameStatus status)
            => streetName.Status = status;

        private async Task Produce(
            ProducerContext context,
            int streetNamePersistentLocalId,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var streetName = await context.StreetNames.FindAsync(streetNamePersistentLocalId, cancellationToken: cancellationToken)
                                    ?? throw new ProjectionItemNotFoundException<ProducerProjections>(streetNamePersistentLocalId.ToString());

            if (!RegionFilter.IsFlemishRegion(streetName.NisCode))
            {
                return;
            }

            var streetNameLdes = new StreetNameLdes(streetName, _osloNamespace);

            await Produce(
                $"{_osloNamespace}/{streetName.StreetNamePersistentLocalId}",
                streetName.StreetNamePersistentLocalId.ToString(),
                JsonConvert.SerializeObject(streetNameLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task Produce(
            string puri,
            string objectId,
            string jsonContent,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
