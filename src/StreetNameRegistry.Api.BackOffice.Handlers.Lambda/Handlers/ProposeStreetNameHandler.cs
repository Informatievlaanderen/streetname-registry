namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Convertors;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class ProposeStreetNameHandler : StreetNameLambdaHandler<ProposeStreetNameLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeStreetNameHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            IMunicipalities municipalities)
            : base(
                configuration,
                retryPolicy,
                municipalities,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<object> InnerHandle(ProposeStreetNameLambdaRequest request, CancellationToken cancellationToken)
        {
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

            var nisCode = request.Request.GemeenteId
                .AsIdentifier()
                .Map(IdentifierMappings.MunicipalityNisCode);

            await _backOfficeContext
                .AddIdempotentMunicipalityStreetNameIdRelation(
                    request.PersistentLocalId,
                    request.MunicipalityId(),
                    nisCode,
                    cancellationToken);

            var lastHash = await GetStreetNameHash(request.MunicipalityId(), request.PersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.PersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeStreetNameLambdaRequest request)
        {
            return exception switch
            {
                StreetNameNameAlreadyExistsException nameExists =>
                    ValidationErrors.Common.StreetNameAlreadyExists.ToTicketError(nameExists.Name),
                MunicipalityHasInvalidStatusException =>
                    ValidationErrors.Common.MunicipalityHasInvalidStatus.ToTicketError(),
                StreetNameNameLanguageIsNotSupportedException _ =>
                    ValidationErrors.Common.StreetNameNameLanguageIsNotSupported.ToTicketError(),
                StreetNameIsMissingALanguageException _ =>
                    ValidationErrors.ProposeStreetName.StreetNameIsMissingALanguage.ToTicketError(),
                _ => null
            };
        }
    }
}
