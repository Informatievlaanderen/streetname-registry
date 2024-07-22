namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class CorrectStreetNameRejectionHandler : StreetNameLambdaHandler<CorrectStreetNameRejectionLambdaRequest>
    {
        public CorrectStreetNameRejectionHandler(
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

        protected override async Task<object> InnerHandle(CorrectStreetNameRejectionLambdaRequest request, CancellationToken cancellationToken)
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

            var lastHash = await GetStreetNameHash(request.MunicipalityId(), streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, streetNamePersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectStreetNameRejectionLambdaRequest request)
        {
            return exception switch
            {
                MunicipalityHasInvalidStatusException =>
                    ValidationErrors.Common.MunicipalityStatusNotCurrent.ToTicketError(),
                StreetNameHasInvalidStatusException =>
                    ValidationErrors.CorrectStreetNameRejection.InvalidStatus.ToTicketError(),
                StreetNameNameAlreadyExistsException streetNameNameAlreadyExistsException  =>
                    ValidationErrors.Common.StreetNameAlreadyExists.ToTicketError(streetNameNameAlreadyExistsException.Name),
                _ => null
            };
        }
    }
}
