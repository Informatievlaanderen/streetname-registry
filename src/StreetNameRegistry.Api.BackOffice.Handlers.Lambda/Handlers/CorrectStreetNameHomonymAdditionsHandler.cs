namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
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

    public sealed class
        CorrectStreetNameHomonymAdditionsHandler : StreetNameLambdaHandler<
            CorrectStreetNameHomonymAdditionsLambdaRequest>
    {
        public CorrectStreetNameHomonymAdditionsHandler(
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
        {
        }

        protected override async Task<ETagResponse> InnerHandle(CorrectStreetNameHomonymAdditionsLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new PersistentLocalId(request.StreetNamePersistentLocalId);
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

            var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, streetNamePersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception,
            CorrectStreetNameHomonymAdditionsLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => ValidationErrors.CorrectStreetNameHomonymAdditions.InvalidStatus.ToTicketError(),

                StreetNameNameAlreadyExistsException =>
                    ValidationErrors.CorrectStreetNameHomonymAdditions.StreetNameNameWithHomonymAdditionAlreadyExists.ToTicketError(),

                CannotAddHomonymAdditionException e =>
                    ValidationErrors.CorrectStreetNameHomonymAdditions.CannotAddHomonymAddition.ToTicketError(e.Language),

                HomonymAdditionMaxCharacterLengthExceededException e =>
                    ValidationErrors.CorrectStreetNameHomonymAdditions.HomonymAdditionMaxCharacterLengthExceeded.ToTicketError(e.Language, e.NumberOfCharacters),

                _ => null
            };
        }
    }
}
