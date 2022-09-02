namespace StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Municipality;
    using Requests;
    using TicketingService.Abstractions;
    using MunicipalityId = Municipality.MunicipalityId;

    public class SqsStreetNameCorrectNamesHandler : SqsLambdaHandler<SqsLambdaStreetNameCorrectNamesRequest>
    {
        private readonly IMunicipalities _municipalities;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsStreetNameCorrectNamesHandler(
            ITicketing ticketing,
            ICommandHandlerResolver bus,
            IMunicipalities municipalities,
            IdempotencyContext idempotencyContext)
            : base(ticketing, bus)
        {
            _municipalities = municipalities;
            _idempotencyContext = idempotencyContext;
        }

        protected override async Task<string> InnerHandle(SqsLambdaStreetNameCorrectNamesRequest request, CancellationToken cancellationToken)
        {
            var municipalityId = new MunicipalityId(Guid.Parse(request.MessageGroupId));
            var streetNamePersistentLocalId = new PersistentLocalId(request.Request.PersistentLocalId);

            var cmd = request.Request.ToCommand(
                municipalityId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            var lastEventHash = await GetStreetNameHash(_municipalities, municipalityId, streetNamePersistentLocalId, cancellationToken);

            return lastEventHash;
        }
    }
}
