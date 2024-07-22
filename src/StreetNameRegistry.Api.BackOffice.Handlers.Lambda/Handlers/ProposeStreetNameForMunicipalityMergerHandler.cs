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
    using Municipality.Commands;
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

        protected override async Task<object> InnerHandle(
            ProposeStreetNameForMunicipalityMergerLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var commands = await BuildCommands(request, cancellationToken);

            try
            {
                foreach (var command in commands)
                {
                    await IdempotentCommandHandler.Dispatch(
                        command.CreateCommandId(),
                        command,
                        request.Metadata!,
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

        private async Task<IList<ProposeStreetNameForMunicipalityMerger>> BuildCommands(
            ProposeStreetNameForMunicipalityMergerLambdaRequest request,
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
                        request.MunicipalityPersistentLocalId(),
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
            ProposeStreetNameForMunicipalityMergerLambdaRequest request)
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
