namespace StreetNameRegistry.Api.BackOffice.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Requests;
    using Abstractions.Response;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Consumer;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Municipality;

    public class ProposeStreetNameHandler : BusHandler, IRequestHandler<StreetNameProposeRequest, PersistentLocalIdETagResponse>
    {
        private readonly ConsumerContext _consumerContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IMunicipalities _municipalities;

        public ProposeStreetNameHandler(
            ICommandHandlerResolver bus,
            ConsumerContext consumerContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IdempotencyContext idempotencyContext,
            BackOfficeContext backOfficeContext,
            IMunicipalities municipalities
            )
            : base(bus)
        {
            _consumerContext = consumerContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _idempotencyContext = idempotencyContext;
            _backOfficeContext = backOfficeContext;
            _municipalities = municipalities;
        }

        public async Task<PersistentLocalIdETagResponse> Handle(StreetNameProposeRequest request, CancellationToken cancellationToken)
        {
            var identifier = request.GemeenteId
                .AsIdentifier()
                .Map(IdentifierMappings.MunicipalityNisCode);

            var municipality = await _consumerContext.MunicipalityConsumerItems
                .AsNoTracking()
                .SingleOrDefaultAsync(item =>
                    item.NisCode == identifier.Value, cancellationToken);

            if (municipality == null)
            {
                throw new InvalidOperationException();
            }

            var persistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId();
            var municipalityId = new MunicipalityId(municipality.MunicipalityId);

            var cmd = request.ToCommand(municipalityId, CreateFakeProvenance(), persistentLocalId);
            await IdempotentCommandHandlerDispatch(_idempotencyContext, cmd.CreateCommandId(), cmd, request.Metadata, cancellationToken);

            // Insert PersistentLocalId with MunicipalityId
            await _backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .AddAsync(new MunicipalityIdByPersistentLocalId(persistentLocalId, municipalityId), cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var streetNameHash = await GetStreetNameHash(_municipalities, municipalityId, persistentLocalId, cancellationToken);

            return new PersistentLocalIdETagResponse(persistentLocalId, streetNameHash);
        }
    }
}
