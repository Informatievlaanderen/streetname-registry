namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class ProposeStreetNameHandler : StreetNameLambdaHandler<ProposeStreetNameLambdaRequest>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeStreetNameHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
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
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<ETagResponse> InnerHandle(ProposeStreetNameLambdaRequest request, CancellationToken cancellationToken)
        {
            var persistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId();

            var cmd = request.ToCommand(persistentLocalId);

            await IdempotentCommandHandler.Dispatch(
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            // Insert PersistentLocalId with MunicipalityId
            await _backOfficeContext
                .AddIdempotentMunicipalityStreetNameIdRelation(persistentLocalId, request.MunicipalityPersistentLocalId(), cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), persistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, persistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeStreetNameLambdaRequest request)
        {
            return exception switch
            {
                StreetNameNameAlreadyExistsException nameExists => new TicketError(
                    ValidationErrors.Common.StreetNameAlreadyExists.Message(nameExists.Name),
                    ValidationErrors.Common.StreetNameAlreadyExists.Code),
                MunicipalityHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.MunicipalityHasInvalidStatus.Message,
                    ValidationErrors.Common.MunicipalityHasInvalidStatus.Code),
                StreetNameNameLanguageIsNotSupportedException _ => new TicketError(
                    ValidationErrors.Common.StreetNameNameLanguageIsNotSupported.Message,
                    ValidationErrors.Common.StreetNameNameLanguageIsNotSupported.Code),
                StreetNameIsMissingALanguageException _ => new TicketError(
                    ValidationErrors.ProposeStreetName.StreetNameIsMissingALanguage.Message,
                    ValidationErrors.ProposeStreetName.StreetNameIsMissingALanguage.Code),
                _ => null
            };
        }
    }
}
