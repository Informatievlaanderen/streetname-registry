namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Configuration;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public abstract class StreetNameLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        private readonly IMunicipalities _municipalities;

        protected string DetailUrlFormat { get; }

        protected StreetNameLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IMunicipalities municipalities,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
            _municipalities = municipalities;

            DetailUrlFormat = configuration["DetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'DetailUrl' cannot be found in the configuration");
            }
        }

        protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not IHasStreetNamePersistentLocalId id)
            {
                return;
            }

            var latestEventHash = await GetStreetNameHash(
                request.MunicipalityPersistentLocalId(),
                new PersistentLocalId(id.StreetNamePersistentLocalId),
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

            if (request.IfMatchHeaderValue != lastHashTag.ToString())
            {
                throw new IfMatchHeaderValueMismatchException();
            }
        }

        protected async Task<string> GetStreetNameHash(
            MunicipalityId municipalityId,
            PersistentLocalId persistentLocalId,
            CancellationToken cancellationToken)
        {
            var municipality = await _municipalities.GetAsync(new MunicipalityStreamId(municipalityId), cancellationToken);
            var streetNameHash = municipality.GetStreetNameHash(persistentLocalId);
            return streetNameHash;
        }

        protected override async Task HandleAggregateIdIsNotFoundException(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            await Ticketing.Error(request.TicketId,
                new TicketError(
                    ValidationErrors.Common.StreetNameNotFound.Message,
                    ValidationErrors.Common.StreetNameNotFound.Code),
                cancellationToken);
        }

        protected abstract TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request);

        protected override TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
        {
            var error = InnerMapDomainException(exception, request);
            if (error is not null)
            {
                return error;
            }

            return exception switch
            {
                StreetNameIsNotFoundException => new TicketError(
                    ValidationErrors.Common.StreetNameNotFound.Message,
                    ValidationErrors.Common.StreetNameNotFound.Code),
                StreetNameIsRemovedException => new TicketError(
                    ValidationErrors.Common.StreetNameIsRemoved.Message,
                    "VerwijderdeStraatnaam"),
                _ => null
            };
        }
    }
}
