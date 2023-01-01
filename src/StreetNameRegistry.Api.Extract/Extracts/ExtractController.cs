namespace StreetNameRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Projections.Extract;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Infrastructure.FeatureToggles;
    using Projections.Syndication.Microsoft;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        public static readonly string ZipName = "Straatnaam";

        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="useExtractV2Toggle"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als straatnaamregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public IActionResult Get(
            [FromServices] ExtractContext context,
            [FromServices] UseExtractV2Toggle useExtractV2Toggle,
            [FromServices] SyndicationContext syndicationContext,
            CancellationToken cancellationToken = default)
        {
            if (useExtractV2Toggle.FeatureEnabled)
            {
                return new IsolationExtractArchive ($"{ZipName}-{DateTime.Now:yyyy-MM-dd}", context) { StreetNameRegistryExtractBuilder.CreateStreetNameFilesV2(context, syndicationContext) }
                    .CreateFileCallbackResult(cancellationToken);
            }
            return new IsolationExtractArchive ($"{ZipName}-{DateTime.Now:yyyy-MM-dd}", context) { StreetNameRegistryExtractBuilder.CreateStreetNameFiles(context, syndicationContext) }
                .CreateFileCallbackResult(cancellationToken);
        }
    }
}
