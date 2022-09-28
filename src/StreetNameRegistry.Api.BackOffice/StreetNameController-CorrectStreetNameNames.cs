namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Corrigeer de straatnaam van een straatnaam.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="validator"></param>
        /// <param name="options"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot correctie wordt reeds verwerkt.</response>
        /// <response code="400">Als de straatnaam status niet 'voorgesteld' of 'inGebruik' is.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/corrigeren/straatnaam")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectStreetNameNames(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IValidator<StreetNameCorrectNamesRequest> validator,
            [FromServices] IOptions<ResponseOptions> options,
            [FromRoute] int persistentLocalId,
            [FromBody] StreetNameCorrectNamesRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            request.PersistentLocalId = persistentLocalId;

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new PersistentLocalId(request.PersistentLocalId), cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                if (_useSqsToggle.FeatureEnabled)
                {
                    var result = await _mediator.Send(
                        new SqsStreetNameCorrectNamesRequest
                        {
                            Request = request,
                            PersistentLocalId = persistentLocalId,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                            IfMatchHeaderValue = ifMatchHeaderValue
                        }, cancellationToken);

                    return Accepted(result);
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.DetailUrl, request.PersistentLocalId)),
                    response.ETag);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.StreetName.StreetNameNotFound, StatusCodes.Status404NotFound);
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

                    StreetNameIsNotFoundException => new ApiException(ValidationErrorMessages.StreetName.StreetNameNotFound, StatusCodes.Status404NotFound),

                    StreetNameIsRemovedException => new ApiException(ValidationErrorMessages.StreetName.StreetNameIsRemoved, StatusCodes.Status410Gone),

                    StreetNameHasInvalidStatusException => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameCannotBeCorrected,
                        String.Empty,
                        ValidationErrorMessages.StreetName.StreetNameHasInvalidStatus),

                    StreetNameNameLanguageIsNotSupportedException _ => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameNameLanguageIsNotSupported,
                        nameof(request.Straatnamen),
                        ValidationErrorMessages.StreetName.StreetNameNameLanguageIsNotSupported),

                    _ => new ValidationException(new List<ValidationFailure>
                    {
                        new ValidationFailure(string.Empty, exception.Message)
                    })
                };
            }
        }
    }
}
