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

    public sealed class CorrectStreetNameNamesHandler : StreetNameLambdaHandler<CorrectStreetNameNamesLambdaRequest>
    {
        public CorrectStreetNameNamesHandler(
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

        protected override async Task<ETagResponse> InnerHandle(CorrectStreetNameNamesLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectStreetNameNamesLambdaRequest request)
        {
            return exception switch
            {
                StreetNameNameAlreadyExistsException nameExists => new TicketError(
                    ValidationErrors.Common.StreetNameAlreadyExists.Message(nameExists.Name),
                    ValidationErrors.Common.StreetNameAlreadyExists.Code),
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectStreetNameNames.InvalidStatus.Message,
                    ValidationErrors.CorrectStreetNameNames.InvalidStatus.Code),
                StreetNameNameLanguageIsNotSupportedException _ => new TicketError(
                    ValidationErrors.Common.StreetNameNameLanguageIsNotSupported.Message,
                    ValidationErrors.Common.StreetNameNameLanguageIsNotSupported.Code),
                _ => null
            };
        }
    }
}
