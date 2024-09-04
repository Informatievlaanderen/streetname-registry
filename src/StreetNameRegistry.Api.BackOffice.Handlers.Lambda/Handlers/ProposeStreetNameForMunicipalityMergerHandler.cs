namespace StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Municipality;
    using Municipality.Commands;
    using Municipality.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class ProposeStreetNameForMunicipalityMergerHandler : StreetNameLambdaHandler<ProposeStreetNamesForMunicipalityMergerLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly ILogger _logger;

        public ProposeStreetNameForMunicipalityMergerHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IScopedIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            IMunicipalities municipalities,
            ILoggerFactory loggerFactory
        )
            : base(
                configuration,
                retryPolicy,
                municipalities,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        protected override async Task<object> InnerHandle(
            ProposeStreetNamesForMunicipalityMergerLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var commands = await BuildCommands(request, cancellationToken);

            foreach (var command in commands)
            {
                _logger.LogDebug($"Handling {command.GetType().FullName}");

                try
                {
                    await IdempotentCommandHandler.Dispatch(
                        command.CreateCommandId(),
                        command,
                        request.Metadata!,
                        cancellationToken: cancellationToken);
                    _logger.LogDebug($"Handled {command.GetType().FullName}");
                }
                catch (IdempotencyException)
                {
                    // Idempotent: Do Nothing return last etag
                    _logger.LogDebug($"Skipped due to idempotency {command.GetType().FullName}");
                }
            }

            var etagResponses = new List<ETagResponse>();

            foreach (var command in commands)
            {
                await _backOfficeContext
                    .AddIdempotentMunicipalityStreetNameIdRelation(
                        command.PersistentLocalId,
                        request.MunicipalityId(),
                        request.NisCode,
                        cancellationToken);

                var lastHash = await GetStreetNameHash(request.MunicipalityId(), command.PersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, command.PersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        private async Task<IList<ProposeStreetNameForMunicipalityMerger>> BuildCommands(
            ProposeStreetNamesForMunicipalityMergerLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var oldMunicipalities = new List<Municipality>();
            foreach (var municipalityId in request.StreetNames
                         .SelectMany(x => x.MergedStreetNames.Select(y => y.MunicipalityId))
                         .Distinct())
            {
                oldMunicipalities.Add(await Municipalities.GetAsync(
                    new MunicipalityStreamId(new MunicipalityId(municipalityId)), cancellationToken));
            }

            return request.StreetNames
                .Select(streetName =>
                {
                    var desiredStatus = streetName.MergedStreetNames
                        .Any(mergedStreetName =>
                        {
                            return oldMunicipalities
                                .Single(x => x.MunicipalityId == mergedStreetName.MunicipalityId)
                                .StreetNames.Any(y =>
                                    y.PersistentLocalId == mergedStreetName.StreetNamePersistentLocalId
                                    && y.Status == StreetNameStatus.Current);
                        })
                        ? StreetNameStatus.Current
                        : StreetNameStatus.Proposed;

                    return new ProposeStreetNameForMunicipalityMerger(
                        request.MunicipalityId(),
                        desiredStatus,
                        new Names(new[] { new StreetNameName(streetName.StreetName, Language.Dutch) }),
                        streetName.HomonymAddition is not null
                            ? new HomonymAdditions(new[] { new StreetNameHomonymAddition(streetName.HomonymAddition, Language.Dutch) })
                            : null,
                        new PersistentLocalId(streetName.NewPersistentLocalId),
                        streetName.MergedStreetNames.Select(x => new PersistentLocalId(x.StreetNamePersistentLocalId)).ToList(),
                        request.Provenance);
                })
                .ToList();
        }

        protected override TicketError? InnerMapDomainException(DomainException exception,
            ProposeStreetNamesForMunicipalityMergerLambdaRequest request)
        {
            return exception switch
            {
                StreetNameNameAlreadyExistsException nameExists =>
                    ValidationErrors.Common.StreetNameAlreadyExists.ToTicketError(nameExists.Name),
                MunicipalityHasInvalidStatusException =>
                    ValidationErrors.Common.MunicipalityHasInvalidStatus.ToTicketError(),
                StreetNameHasInvalidDesiredStatusException =>
                    new TicketError("Desired status should be proposed or current", "StreetNameHasInvalidDesiredStatus"),
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
