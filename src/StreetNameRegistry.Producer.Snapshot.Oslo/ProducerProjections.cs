namespace StreetNameRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Municipality.Events;

    [ConnectedProjectionName("Kafka producer snapshot oslo")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string StreetNameTopicKey = "StreetNameTopic";

        private readonly IProducer _producer;

        public ProducerProjections(
            IProducer producer,
            ISnapshotManager snapshotManager,
            string osloNamespace,
            IOsloProxy osloProxy)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameOsloSnapshotsWereRequested>>(async (_, message, ct) =>
            {
                foreach (var persistentLocalId in message.Message.PersistentLocalIds)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await FindAndProduce(async () =>
                            await osloProxy.GetSnapshot(persistentLocalId.ToString(), ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRenamed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        message.Message.GetHash(),
                        message.Position,
                        throwStaleWhenGone: false,
                        ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.PersistentLocalId}", message.Message.PersistentLocalId.ToString(),"{}", message.Position, ct);
            });


            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityNisCodeWasChanged>>(async (_, message, ct) =>
            {
                foreach (var persistentLocalId in message.Message.StreetNamePersistentLocalIds)
                {
                    await Produce($"{osloNamespace}/{persistentLocalId}", persistentLocalId.ToString(),"{}", message.Position, ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityBecameCurrent>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityOfficialLanguageWasAdded>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityOfficialLanguageWasRemoved>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasImported>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasMerged>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasNamed>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityWasRetired>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereCorrected>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);
        }

        private async Task FindAndProduce(
            Func<Task<OsloResult?>> findMatchingSnapshot,
            long storePosition,
            CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
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
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
