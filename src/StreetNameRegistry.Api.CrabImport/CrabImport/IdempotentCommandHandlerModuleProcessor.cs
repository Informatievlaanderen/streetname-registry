namespace StreetNameRegistry.Api.CrabImport.CrabImport
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using SqlStreamStore;
    using StreetName;
    using StreetName.Commands;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly StreetNameCommandHandlerModule _streetNameCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, StreetName, Provenance> _provenanceFactory;
        private readonly Func<IStreetNames> _getStreetNames;

        public IdempotentCommandHandlerModuleProcessor(
            Func<IStreetNames> getStreetNames,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            StreetNameProvenanceFactory provenanceFactory)
        {
            _getStreetNames = getStreetNames;
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _provenanceFactory = provenanceFactory.CreateFrom;

            _streetNameCommandHandlerModule = new StreetNameCommandHandlerModule(
                getStreetNames,
                () => concurrentUnitOfWork,
                getStreamStore,
                eventMapping,
                eventSerializer,
                provenanceFactory);
        }

        public async Task<CommandMessage> Process(
            dynamic commandToProcess,
            IDictionary<string, object> metadata,
            int currentPosition,
            CancellationToken cancellationToken)
        {
            switch (commandToProcess)
            {
                case ImportStreetNameFromCrab command:
                    var commandImportStreetName = new CommandMessage<ImportStreetNameFromCrab>(command.CreateCommandId(), command, metadata);
                    await _streetNameCommandHandlerModule.ImportStreetNameFromCrab(_getStreetNames, commandImportStreetName, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandImportStreetName, _provenanceFactory, currentPosition);
                    return commandImportStreetName;

                case ImportStreetNameStatusFromCrab command:
                    var commandImportStreetNameStatus = new CommandMessage<ImportStreetNameStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _streetNameCommandHandlerModule.ImportStreetNameStatusFromCrab(_getStreetNames, commandImportStreetNameStatus, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandImportStreetNameStatus, _provenanceFactory, currentPosition);
                    return commandImportStreetNameStatus;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
