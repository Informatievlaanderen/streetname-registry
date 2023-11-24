namespace StreetNameRegistry.StreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    [Obsolete("This is a legacy commandhandler and should not be used anymore.")]
    public sealed class CrabStreetNameCommandHandlerModule : CommandHandlerModule
    {
        public CrabStreetNameCommandHandlerModule(
            Func<IStreetNames> getStreetNames,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            CrabStreetNameProvenanceFactory crabProvenanceFactory,
            StreetNameLegacyProvenanceFactory streetNameProvenanceFactory)
        {
            For<ImportStreetNameFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, crabProvenanceFactory)
                .Handle(async (message, ct) => { await ImportStreetNameFromCrab(getStreetNames, message, ct); });

            For<ImportStreetNameStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, crabProvenanceFactory)
                .Handle(async (message, ct) => { await ImportStreetNameStatusFromCrab(getStreetNames, message, ct); });

            For<MarkStreetNameMigrated>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, streetNameProvenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNames = getStreetNames();
                    var streetName = await streetNames.GetAsync(message.Command.StreetNameId, ct);

                    streetName.MarkMigrated(message.Command.MunicipalityId);
                });
        }

        public async Task ImportStreetNameFromCrab(
            Func<IStreetNames> getStreetNames,
            CommandMessage<ImportStreetNameFromCrab> message,
            CancellationToken ct)
        {
            var streetNames = getStreetNames();

            var streetNameId = StreetNameId.CreateFor(message.Command.StreetNameId);
            var municipalityId = MunicipalityId.CreateFor(message.Command.MunicipalityId);

            var streetName =
                await GetOrRegisterStreetName(
                    streetNames,
                    streetNameId,
                    municipalityId,
                    message.Command.NisCode,
                    ct);

            ImportFromCrab(streetName.Value, message.Command);
        }

        public async Task ImportStreetNameStatusFromCrab(
            Func<IStreetNames> getStreetNames,
            CommandMessage<ImportStreetNameStatusFromCrab> message,
            CancellationToken ct)
        {
            var streetNames = getStreetNames();

            var streetNameId = StreetNameId.CreateFor(message.Command.StreetNameId);

            var streetName = await streetNames.GetAsync(streetNameId, ct);

            ImportStatusFromCrab(streetName, message.Command);
        }

        private static void ImportFromCrab(StreetName streetName, ImportStreetNameFromCrab command)
        {
            streetName.ImportFromCrab(
                command.StreetNameId,
                command.MunicipalityId,
                command.PrimaryStreetName,
                command.SecondaryStreetName,
                command.PrimaryTransStreetName,
                command.SecondaryTransStreetName,
                command.PrimaryLanguage,
                command.SecondaryLanguage,
                command.LifeTime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        private static void ImportStatusFromCrab(StreetName streetName, ImportStreetNameStatusFromCrab command)
        {
            streetName.ImportStatusFromCrab(
                command.StreetNameStatusId,
                command.StreetNameId,
                command.StreetNameStatus,
                command.LifeTime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        private static async Task<Optional<StreetName>> GetOrRegisterStreetName(
            IStreetNames streetNames,
            StreetNameId streetNameId,
            MunicipalityId municipalityId,
            NisCode nisCode,
            CancellationToken ct)
        {
            var streetName = await streetNames.GetOptionalAsync(streetNameId, ct);

            if (streetName.HasValue)
                return streetName;

            streetName = new Optional<StreetName>(
                StreetName.Register(
                    new StreetNameId(streetNameId),
                    new MunicipalityId(municipalityId),
                    nisCode));

            streetNames.Add(streetNameId, streetName.Value);

            return streetName;
        }
    }
}
