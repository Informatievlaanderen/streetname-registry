namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Configuration;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using StreetNameRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public abstract class SqsLambdaHandler<TSqsLambdaRequest> : IRequestHandler<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        private readonly ITicketing _ticketing;
        private readonly ICustomRetryPolicy _retryPolicy;
        private readonly IMunicipalities _municipalities;

        protected IIdempotentCommandHandler IdempotentCommandHandler { get; }
        protected string DetailUrlFormat { get; }

        protected SqsLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IMunicipalities municipalities,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
        {
            _retryPolicy = retryPolicy;
            _municipalities = municipalities;
            _ticketing = ticketing;
            IdempotentCommandHandler = idempotentCommandHandler;

            DetailUrlFormat = configuration["DetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'DetailUrl' cannot be found in the configuration");
            }
        }

        protected abstract Task<ETagResponse> InnerHandle(TSqsLambdaRequest request, CancellationToken cancellationToken);

        protected abstract TicketError? InnerMapDomainException(DomainException exception);

        protected TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
        {
            var error = InnerMapDomainException(exception);
            return error ?? null;
        }

        public async Task<Unit> Handle(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateIfMatchHeaderValue(request, cancellationToken);

                await _ticketing.Pending(request.TicketId, cancellationToken);

                ETagResponse? etag = null;

                await _retryPolicy.Retry(async () => etag = await InnerHandle(request, cancellationToken));

                await _ticketing.Complete(
                    request.TicketId,
                    new TicketResult(etag),
                    cancellationToken);
            }
            catch (IfMatchHeaderValueMismatchException)
            {
                await _ticketing.Error(
                    request.TicketId,
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    cancellationToken);
            }
            catch (DomainException exception)
            {
                var ticketError = exception switch
                {
                    StreetNameIsNotFoundException => new TicketError(
                        ValidationErrorMessages.StreetName.StreetNameNotFound,
                        ValidationErrorCodes.StreetName.StreetNameNotFound),
                    StreetNameIsRemovedException => new TicketError(
                        ValidationErrorMessages.StreetName.StreetNameIsRemoved,
                        ValidationErrorCodes.StreetName.StreetNameIsRemoved),
                    _ => InnerMapDomainException(exception)
                };

                ticketError ??= new TicketError(exception.Message, "");

                await _ticketing.Error(
                    request.TicketId,
                    ticketError,
                    cancellationToken);
            }

            return Unit.Value;
        }

        private async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
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
    }
}
