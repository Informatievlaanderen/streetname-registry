namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Response;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using StreetNameRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public sealed class SqsStreetNameCorrectRejectionLambdaHandler : SqsLambdaHandler<SqsLambdaStreetNameCorrectRejectionRequest>
    {
        public SqsStreetNameCorrectRejectionLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IMunicipalities municipalities,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                municipalities,
                ticketing,
                idempotentCommandHandler)
        { }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaStreetNameCorrectRejectionRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new PersistentLocalId(request.Request.PersistentLocalId);
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetStreetNameHash(request.MunicipalityId, streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, streetNamePersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception)
        {
            return exception switch
            {
                MunicipalityHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Municipality.MunicipalityStatusNotCurrent,
                    ValidationErrorCodes.Municipality.MunicipalityStatusNotCurrent),
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameRejectionCannotBeCorrect,
                    ValidationErrorCodes.StreetName.StreetNameRejectionCannotBeCorrect),
                StreetNameNameAlreadyExistsException streetNameNameAlreadyExistsException  => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameAlreadyExists(streetNameNameAlreadyExistsException.Name),
                    ValidationErrorCodes.StreetName.StreetNameAlreadyExists),
                _ => null
            };
        }
    }
}
