namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Municipality.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Stel een straatnaam voor.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="201">Als de straatnaam voorgesteld is.</response>
        /// <response code="202">Als de straatnaam reeds voorgesteld is.</response>
        /// <returns></returns>
        [HttpPost("acties/voorstellen")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, "location", "string", "De url van de voorgestelde straatnaam.")]
        [SwaggerRequestExample(typeof(StreetNameProposeRequest), typeof(StreetNameProposeRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Propose(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<StreetNameProposeRequest> validator,
            [FromBody] StreetNameProposeRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                if (_useSqsToggle.FeatureEnabled)
                {
                    var result = await _mediator.Send(
                        new SqsStreetNameProposeRequest
                        {
                            Request = request,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                        }, cancellationToken);

                    return Accepted(result);
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new CreatedWithLastObservedPositionAsETagResult(
                    new Uri(string.Format(options.Value.DetailUrl, response.PersistentLocalId)),
                    response.LastEventHash);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.StreetName.StreetNameMunicipalityUnknown,
                    string.Empty,
                    ValidationErrorMessages.StreetName.StreetNameMunicipalityUnknown(request.GemeenteId));
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.StreetName.StreetNameMunicipalityUnknown,
                    string.Empty,
                    ValidationErrorMessages.StreetName.StreetNameMunicipalityUnknown(request.GemeenteId));
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    StreetNameNameAlreadyExistsException nameExists => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameAlreadyExists,
                        nameof(request.Straatnamen),
                        ValidationErrorMessages.StreetName.StreetNameAlreadyExists(nameExists.Name)),

                    MunicipalityHasInvalidStatusException _ => CreateValidationException(
                        ValidationErrorCodes.Municipality.MunicipalityHasInvalidStatus,
                        nameof(request.GemeenteId),
                        ValidationErrorMessages.Municipality.MunicipalityHasInvalidStatus),

                    StreetNameNameLanguageIsNotSupportedException _ => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameNameLanguageIsNotSupported,
                        nameof(request.Straatnamen),
                        ValidationErrorMessages.StreetName.StreetNameNameLanguageIsNotSupported),

                    StreetNameIsMissingALanguageException _ => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameIsMissingALanguage,
                        nameof(request.Straatnamen),
                        ValidationErrorMessages.StreetName.StreetNameIsMissingALanguage),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
