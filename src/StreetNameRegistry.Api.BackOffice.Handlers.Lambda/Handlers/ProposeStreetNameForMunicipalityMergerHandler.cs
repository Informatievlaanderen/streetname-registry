namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
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

    public sealed class ProposeStreetNameForMunicipalityMergerHandler : StreetNameLambdaHandler<ProposeStreetNameForMunicipalityMergerLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeStreetNameForMunicipalityMergerHandler(
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

        protected override async Task<object> InnerHandle(ProposeStreetNameForMunicipalityMergerLambdaRequest request, CancellationToken cancellationToken)
        {
            var commands = request.ToCommand().ToList();

            try
            {
                foreach (var command in commands)
                {
                    await IdempotentCommandHandler.Dispatch(
                        command.CreateCommandId(),
                        command,
                        request.Metadata,
                        cancellationToken);
                }
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var etagResponses = new List<ETagResponse>();

            foreach (var command in commands)
            {
                await _backOfficeContext
                    .AddIdempotentMunicipalityStreetNameIdRelation(
                        command.PersistentLocalId,
                        request.MunicipalityPersistentLocalId(),
                        request.NisCode,
                        cancellationToken);

                var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), command.PersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, command.PersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeStreetNameForMunicipalityMergerLambdaRequest request)
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
                MergedStreetNamePersistentLocalIdsAreMissingException =>
                    new TicketError("MergedStreetNamePersistentLocalIdsAreMissing", "MergedStreetNamePersistentLocalIdsAreMissing"),
                MergedStreetNamePersistentLocalIdsAreNotUniqueException =>
                    new TicketError("MergedStreetNamePersistentLocalIdsAreNotUnique", "MergedStreetNamePersistentLocalIdsAreNotUnique"),
                _ => null
            };
        }
    }
}
