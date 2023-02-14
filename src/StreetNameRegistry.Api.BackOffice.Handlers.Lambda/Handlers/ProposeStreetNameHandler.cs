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
    using Municipality.Commands;
    using Municipality.Exceptions;
    using Requests;
    using StreetNameRegistry.StreetName;
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

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);

                await _backOfficeContext
                    .AddIdempotentMunicipalityStreetNameIdRelation(persistentLocalId, request.MunicipalityPersistentLocalId(), cancellationToken);
                await _backOfficeContext.SaveChangesAsync(cancellationToken);

                var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), persistentLocalId, cancellationToken);
                return new ETagResponse(string.Format(DetailUrlFormat, persistentLocalId), lastHash);

            }
            catch (IdempotencyException)
            {
                var municipality = await Municipalities.GetAsync(new MunicipalityStreamId(request.MunicipalityPersistentLocalId()), cancellationToken);

                var streetName = GetStreetNameFromMunicipality(municipality, cmd);

                var lastHash = await GetStreetNameHash(request.MunicipalityPersistentLocalId(), streetName.PersistentLocalId, cancellationToken);
                return new ETagResponse(string.Format(DetailUrlFormat, streetName.PersistentLocalId), lastHash);
            }
        }

        private static MunicipalityStreetName GetStreetNameFromMunicipality(Municipality municipality, ProposeStreetName proposeStreetName)
        {
            var streetName = municipality.StreetNames
                .Where(x => !x.IsRejected && !x.IsRetired && !x.IsRemoved && !x.HomonymAdditions.Any())
                .Single(x => proposeStreetName.StreetNameNames.Any(y => x.Names.HasMatch(y.Language, y.Name)));
            return streetName;
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
