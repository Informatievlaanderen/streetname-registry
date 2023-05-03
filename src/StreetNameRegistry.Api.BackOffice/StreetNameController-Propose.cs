namespace StreetNameRegistry.Api.BackOffice
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using Infrastructure.Authorization;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Stel een straatnaam voor.
        /// </summary>
        /// <param name="proposeStreetNameRequestFactory"></param>
        /// <param name="request"></param>
        /// <param name="nisCodeAuthorizer"></param>
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
            [FromServices] INisCodeAuthorizer<MunicipalityPuri> nisCodeAuthorizer,
            [FromServices] IValidator<ProposeStreetNameRequest> validator,
            [FromServices] ProposeStreetNameRequestFactory proposeStreetNameRequestFactory,
            [FromBody] ProposeStreetNameRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await nisCodeAuthorizer.IsAuthorized(HttpContext.FindOvoCodeClaim(), new MunicipalityPuri(request.GemeenteId), cancellationToken))
            {
                throw new ApiException(ValidationErrors.NisCodeAuthorization.NotAuthorized.Message, (int)HttpStatusCode.Forbidden);
            }

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                var result = await _mediator.Send(
                    proposeStreetNameRequestFactory.Create(request, GetMetadata(), new ProvenanceData(CreateProvenance(Modification.Insert))),
                    cancellationToken);

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
