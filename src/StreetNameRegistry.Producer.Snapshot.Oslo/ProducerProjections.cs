namespace StreetNameRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Municipality.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string StreetNameTopicKey = "StreetNameTopic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer, ISnapshotManager snapshotManager)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
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
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            //When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRemovedV2>>(async (_, message, ct) =>
            //{
            //    await Produce($"{osloNamespace}/{message.Message.PersistentLocalId}", "{}", ct);
            //});
        }

        private async Task FindAndProduce(
            Func<Task<OsloResult?>> findMatchingSnapshot,
            long storePosition,
            CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, storePosition, ct);
            }
        }

        private async Task Produce(
            string objectId,
            string jsonContent,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(objectId),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
