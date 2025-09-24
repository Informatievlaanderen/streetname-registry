namespace StreetNameRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using MunicipalityDomain = Municipality.Events;

    [ConnectedProjectionName("Kafka producer start vanaf migratie")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt startende vanaf migratie.")]
    public class ProducerMigrateProjections : ConnectedProjection<ProducerContext>
    {
        public const string StreetNameTopicKey = "StreetNameMigrationTopic";

        private readonly IProducer _producer;

        public ProducerMigrateProjections(IProducer producer)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

             When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                 MunicipalityDomain.StreetNameWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
             {
                 await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                     ct);
             });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRenamed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                MunicipalityDomain.StreetNameWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityNisCodeWasChanged>>(
                async (_, message, ct) =>
            {
                await Produce(message.Message.MunicipalityId.ToString(), message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityBecameCurrent>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityOfficialLanguageWasAdded>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityOfficialLanguageWasRemoved>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasImported>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasMerged>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasNamed>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasRetired>>(DoNothing);
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.MunicipalityWasRemoved>>(DoNothing);
        }

        private async Task Produce<T>(
            int persistentLocalId,
            T message,
            long storePosition,
            CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            await Produce(persistentLocalId.ToString(), message, storePosition, cancellationToken);
        }

        private async Task Produce<T>(
            string messageKey,
            T message,
            long storePosition,
            CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(messageKey),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine +
                                                    result.ErrorReason); //TODO: create custom exception
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
