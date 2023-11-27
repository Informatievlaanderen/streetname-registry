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

    public sealed class RenameStreetNameHandler : StreetNameLambdaHandler<RenameStreetNameLambdaRequest>
    {
        public RenameStreetNameHandler(
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

        protected override async Task<ETagResponse> InnerHandle(RenameStreetNameLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata!,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetStreetNameHash(
                request.MunicipalityPersistentLocalId(), new PersistentLocalId(request.StreetNamePersistentLocalId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.StreetNamePersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RenameStreetNameLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException ex when ex.PersistentLocalId == request.StreetNamePersistentLocalId =>
                    ValidationErrors.RenameStreetName.SourceStreetNameHasInvalidStatus.ToTicketError(),
                StreetNameHasInvalidStatusException =>
                    ValidationErrors.RenameStreetName.DestinationStreetNameHasInvalidStatus.ToTicketError(),
                MunicipalityHasInvalidStatusException =>
                    ValidationErrors.Common.MunicipalityStatusNotCurrent.ToTicketError(),
                _ => null
            };
        }
    }
}
