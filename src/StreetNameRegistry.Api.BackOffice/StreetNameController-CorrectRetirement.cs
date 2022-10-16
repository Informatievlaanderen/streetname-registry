namespace StreetNameRegistry.Api.BackOffice
{
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
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Municipality;
    using Municipality.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Corrigeer de opheffing van een straatnaam.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="validator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot correctie opheffing wordt reeds verwerkt.</response>
        /// <response code="204">Als de straatnaam gehistoreerd is.</response>
        /// <response code="400">Als de straatnaam status niet 'voorgesteld' of 'afgekeurd' is.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/corrigeren/opheffing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectRetirement(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IValidator<StreetNameCorrectRetirementRequest> validator,
            [FromRoute] StreetNameCorrectRetirementRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
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
                        new SqsStreetNameCorrectRetirementRequest()
                        {
                            Request = request,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                            IfMatchHeaderValue = ifMatchHeaderValue
                        }, cancellationToken);

                    return Accepted(result);
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new NoContentWithETagResult(response.ETag);
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
                    StreetNameIsNotFoundException => new ApiException(ValidationErrorMessages.StreetName.StreetNameNotFound, StatusCodes.Status404NotFound),

                    StreetNameIsRemovedException => new ApiException(ValidationErrorMessages.StreetName.StreetNameIsRemoved, StatusCodes.Status410Gone),

                    MunicipalityHasInvalidStatusException _ => CreateValidationException(
                        ValidationErrorCodes.Municipality.MunicipalityStatusNotCurrent,
                        string.Empty,
                        ValidationErrorMessages.Municipality.MunicipalityStatusNotCurrent),

                    StreetNameHasInvalidStatusException => CreateValidationException(
                        ValidationErrorCodes.StreetName.StreetNameRetirementCannotBeCorrect,
                        string.Empty,
                        ValidationErrorMessages.StreetName.StreetNameRetirementCannotBeCorrect),

                    _ => new ValidationException(new List<ValidationFailure>
                    {
                        new(string.Empty, exception.Message)
                    })
                };
            }
        }
    }
}
