namespace StreetNameRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Municipality;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Hernoem een straatnaam.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="validator"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/hernoemen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> Rename(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IValidator<RenameStreetNameRequest> validator,
            [FromRoute] int persistentLocalId,
            [FromBody] RenameStreetNameRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            request.StreetNamePersistentLocalId = persistentLocalId;
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue,
                        new PersistentLocalId(persistentLocalId), cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var result = await _mediator.Send(
                    new RenameStreetNameSqsRequest
                    {
                        PersistentLocalId = persistentLocalId,
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateProvenance(Modification.Update)),
                        IfMatchHeaderValue = ifMatchHeaderValue
                    }, cancellationToken);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.StreetNameNotFound.Message, StatusCodes.Status404NotFound);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.StreetNameNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
