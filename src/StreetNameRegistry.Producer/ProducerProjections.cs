namespace StreetNameRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using StreetNameDomain = StreetName.Events;
    using MunicipalityDomain = Municipality.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string _streetNameTopicKey = "StreetNameTopic";

        public ProducerProjections(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var topic = $"{configuration[_streetNameTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {_streetNameTopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameIncomplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameHomonymAdditionWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameNameWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNamePrimaryLanguageWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameSecondaryLanguageWasDefined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameStatusWasCorrectedToRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameStatusWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasNamed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameDomain.StreetNameBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNameId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<MunicipalityDomain.StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.PersistentLocalId, message.Message.ToContract(), ct);
            });
        }

        private async Task Produce<T>(Guid guid, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, guid.ToString("D"), message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private async Task Produce<T>(int persistentLocalId, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, persistentLocalId.ToString(), message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
