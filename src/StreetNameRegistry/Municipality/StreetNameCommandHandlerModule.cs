namespace StreetNameRegistry.Municipality
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    public sealed class StreetNameCommandHandlerModule : CommandHandlerModule
    {
        public StreetNameCommandHandlerModule(
            Func<IMunicipalities> getMunicipalities,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            Func<ISnapshotStore> getSnapshotStore,
            StreetNameProvenanceFactory provenanceFactory)
        {
            For<ProposeStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ProposeStreetName, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.ProposeStreetName(message.Command.StreetNameNames, message.Command.PersistentLocalId);
                });

            For<ApproveStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ApproveStreetName, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.ApproveStreetName(message.Command.PersistentLocalId);
                });

            For<CorrectStreetNameApproval>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectStreetNameApproval, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.CorrectStreetNameApproval(message.Command.PersistentLocalId);
                });

            For<RejectStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RejectStreetName, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.RejectStreetName(message.Command.PersistentLocalId);
                });

            For<CorrectStreetNameRejection>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectStreetNameRejection, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.CorrectStreetNameRejection(message.Command.PersistentLocalId);
                });

            For<MigrateStreetNameToMunicipality>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<MigrateStreetNameToMunicipality, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.MigrateStreetName(
                        message.Command.StreetNameId,
                        message.Command.PersistentLocalId,
                        message.Command.Status,
                        message.Command.PrimaryLanguage,
                        message.Command.SecondaryLanguage,
                        message.Command.Names,
                        message.Command.HomonymAdditions,
                        message.Command.IsCompleted,
                        message.Command.IsRemoved);
                });

            For<RetireStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RetireStreetName, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.RetireStreetName(message.Command.PersistentLocalId);
                });

            For<CorrectStreetNameRetirement>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectStreetNameRetirement, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.CorrectStreetNameRetirement(message.Command.PersistentLocalId);
                });

            For<CorrectStreetNameNames>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectStreetNameNames, Municipality>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var municipality = await getMunicipalities().GetAsync(new MunicipalityStreamId(message.Command.MunicipalityId), ct);
                    municipality.CorrectStreetNameName(message.Command.StreetNameNames, message.Command.PersistentLocalId);
                });
        }
    }
}
