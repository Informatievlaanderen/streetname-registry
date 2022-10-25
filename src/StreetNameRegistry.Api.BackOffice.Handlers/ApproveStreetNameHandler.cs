namespace StreetNameRegistry.Api.BackOffice.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Extensions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using MediatR;
    using Municipality;
    using Municipality.Commands;

    public class ApproveStreetNameHandler : BusHandler, IRequestHandler<StreetNameApproveRequest, ETagResponse>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IMunicipalities _municipalities;
        private readonly IdempotencyContext _idempotencyContext;

        public ApproveStreetNameHandler(
            ICommandHandlerResolver bus,
            BackOfficeContext backOfficeContext,
            IMunicipalities municipalities,
            IdempotencyContext idempotencyContext) : base(bus)
        {
            _backOfficeContext = backOfficeContext;
            _municipalities = municipalities;
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(StreetNameApproveRequest request, CancellationToken cancellationToken)
        {
            var persistentLocalId = new PersistentLocalId(request.PersistentLocalId);

            var municipalityId =
                await _backOfficeContext.GetMunicipalityIdByPersistentLocalId(request.PersistentLocalId);

            var cmd = new ApproveStreetName(municipalityId, persistentLocalId, CreateFakeProvenance());
            await IdempotentCommandHandlerDispatch(_idempotencyContext, cmd.CreateCommandId(), cmd, request.Metadata, cancellationToken);

            var lastEventHash = await GetStreetNameHash(_municipalities, municipalityId, persistentLocalId, cancellationToken);
            return new ETagResponse(string.Empty, lastEventHash);
        }
    }
}
