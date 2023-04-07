namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;

    public partial class StreetNameController
    {
        /// <summary>
        /// Stel een straatnaam voor.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("acties/voorstellen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> Propose(
            [FromServices] IValidator<ProposeStreetNameRequest> validator,
            [FromBody] ProposeStreetNameRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var nisCodeInClaim = User.Claims.FirstOrDefault(x => x.Type == AcmIdmClaimTypes.NisCode)?.Value;
            var nisCodeInRequest = await new NisCodeFinderByPersistentLocalId(_streetNames).FindNisCode(request, cancellationToken);
            if (!nisCodeInClaim.IsValidFor(nisCodeInRequest))
            {
                throw new Exception("zomg! niscode is not allowed");
            }

            try
            {
                var result = await _mediator.Send(
                    new ProposeStreetNameSqsRequest
                    {
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                    }, cancellationToken);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrors.ProposeStreetName.MunicipalityUnknown.Code,
                    string.Empty,
                    ValidationErrors.ProposeStreetName.MunicipalityUnknown.Message(request.GemeenteId));
            }
        }
    }
}
