namespace StreetNameRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using MunicipalityDomain = Municipality.Events;
    using StreetNameDomain = StreetName.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string StreetNameTopicKey = "StreetNameTopic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameIncomplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameStatusWasCorrectedToRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameStatusWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasNamed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(
            Guid guid,
            T message,
            long storePosition,
            CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(guid.ToString("D")),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private async Task Produce<T>(
            int persistentLocalId,
            T message,
            long storePosition,
            CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(persistentLocalId.ToString()),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
