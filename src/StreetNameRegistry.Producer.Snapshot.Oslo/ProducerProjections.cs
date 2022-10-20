namespace StreetNameRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Municipality.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string _streetNameTopicKey = "StreetNameTopic";

        public ProducerProjections(IConfiguration configuration, ISnapshotManager snapshotManager)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            //var osloNamespace = configuration["OsloNamespace"].TrimEnd('/');

            var topic = $"{configuration[_streetNameTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {_streetNameTopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                false,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.PersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        throwStaleWhenGone: false,
                        ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        throwStaleWhenGone: false,
                        ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        throwStaleWhenGone: false,
                        ct),
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.PersistentLocalId.ToString(),
                        message.Message.Provenance.Timestamp,
                        throwStaleWhenGone: false,
                        ct),
                    ct);
            });

            //When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameWasRemovedV2>>(async (_, message, ct) =>
            //{
            //    await Produce($"{osloNamespace}/{message.Message.PersistentLocalId}", "{}", ct);
            //});
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, ct);
            }
        }

        private async Task Produce(string objectId, string jsonContent, CancellationToken cancellationToken = default)
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, objectId, jsonContent, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
