namespace StreetNameRegistry.Api.BackOffice
{
    using System.Net;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Municipality;
    using Municipality.Exceptions;
    using Swashbuckle.AspNetCore.Filters;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Infrastructure.Authorization;

    public partial class StreetNameController
    {
        /// <summary>
        /// Keur een straatnaam af.
        /// </summary>
        /// <param name="nisCodeAuthorizer"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("{persistentLocalId}/acties/afkeuren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> Reject(
            [FromServices] INisCodeAuthorizer<PersistentLocalId> nisCodeAuthorizer,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] RejectStreetNameRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (await nisCodeAuthorizer.IsNotAuthorized(HttpContext, new PersistentLocalId(request.PersistentLocalId), cancellationToken))
                {
                    throw new ApiException(ValidationErrors.NisCodeAuthorization.NotAuthorized.Message, (int)HttpStatusCode.Forbidden);
                }

                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new PersistentLocalId(request.PersistentLocalId), cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var result = await _mediator.Send(
                    new RejectStreetNameSqsRequest
                    {
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
