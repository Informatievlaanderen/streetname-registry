namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class RetireStreetNameHandler : StreetNameLambdaHandler<RetireStreetNameLambdaRequest>
    {
        public RetireStreetNameHandler(
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

        protected override async Task<ETagResponse> InnerHandle(RetireStreetNameLambdaRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new PersistentLocalId(request.Request.PersistentLocalId);
            var cmd = request.ToCommand(streetNamePersistentLocalId);

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

            var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, streetNamePersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RetireStreetNameLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameCannotBeRetired,
                    ValidationErrorCodes.StreetName.StreetNameCannotBeRetired),
                MunicipalityHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Municipality.MunicipalityStatusNotCurrent,
                    ValidationErrorCodes.Municipality.MunicipalityStatusNotCurrent),
                _ => null
            };
        }
    }
}
