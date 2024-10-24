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

    public sealed class ProposeStreetNameForMunicipalityMergerHandler : StreetNameLambdaHandler<ProposeStreetNamesForMunicipalityMergerLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public ProposeStreetNameForMunicipalityMergerHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IScopedIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            IMunicipalities municipalities
        )
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
            ProposeStreetNamesForMunicipalityMergerLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var command = await BuildCommand(request, cancellationToken);

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    command.CreateCommandId(),
                    command,
                    request.Metadata!,
                    cancellationToken: cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: do nothing return last etag
            }

            var etagResponses = new List<ETagResponse>();

            foreach (var streetName in command.StreetNames)
            {
                await _backOfficeContext
                    .AddIdempotentMunicipalityStreetNameIdRelation(
                        streetName.PersistentLocalId,
                        request.MunicipalityId(),
                        request.NisCode,
                        cancellationToken);

                var lastHash = await GetStreetNameHash(request.MunicipalityId(), streetName.PersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, streetName.PersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        private async Task<ProposeStreetNamesForMunicipalityMerger> BuildCommand(
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

            var streetNames = request.StreetNames
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

                    return new ProposeStreetNamesForMunicipalityMerger.StreetNameToPropose(
                        desiredStatus,
                        new Names([new StreetNameName(streetName.StreetName, Language.Dutch)]),
                        streetName.HomonymAddition is not null
                            ? new HomonymAdditions([new StreetNameHomonymAddition(streetName.HomonymAddition, Language.Dutch)])
                            : [],
                        new PersistentLocalId(streetName.NewPersistentLocalId),
                        streetName.MergedStreetNames.Select(x => new PersistentLocalId(x.StreetNamePersistentLocalId)).ToList());
                })
                .ToList();

            return new ProposeStreetNamesForMunicipalityMerger(
                request.MunicipalityId(),
                streetNames,
                request.Provenance);
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
