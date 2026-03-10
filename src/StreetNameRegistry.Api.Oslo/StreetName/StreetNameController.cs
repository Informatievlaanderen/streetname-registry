namespace StreetNameRegistry.Api.Oslo.StreetName
{
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using ChangeFeed;
    using CloudNative.CloudEvents;
    using Count;
    using Detail;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OutputCaching;
    using Microsoft.EntityFrameworkCore;
    using Projections.Feed;
    using Projections.Legacy;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using Sync;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("straatnamen")]
    [ApiExplorerSettings(GroupName = "Straatnamen")]
    public class StreetNameController : ApiController
    {
        private readonly IMediator _mediator;

        public StreetNameController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Vraag een straatnaam op.
        /// </summary>
        /// <param name="persistentLocalId">De persistente lokale identificator van de straatnaam.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de straatnaam gevonden is.</response>
        /// <response code="404">Als de straatnaam niet gevonden kan worden.</response>
        /// <response code="410">Als de straatnaam verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(StreetNameOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(StreetNameNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(StreetNameGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new OsloDetailRequest(persistentLocalId), cancellationToken);

            return string.IsNullOrWhiteSpace(result.LastEventHash)
                ? Ok(result)
                : new OkWithLastObservedPositionAsETagResult(result, result.LastEventHash);
        }

        /// <summary>
        /// Vraag een lijst met straatnamen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met straatnamen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(StreetNameListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var result = await _mediator.Send(new OsloListRequest(filtering, sorting, pagination), cancellationToken);

            Response.AddPaginationResponse(result.Pagination);
            Response.AddSortingResponse(result.Sorting);

            return Ok(result);
        }

        /// <summary>
        /// Vraag het totaal aantal van straatnamen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = Request.ExtractSortingRequest();

            var result = await _mediator.Send(new OsloCountRequest(filtering, sorting), cancellationToken);

            return Ok(result);
        }

        [HttpGet("wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [OutputCache(
            VaryByQueryKeys = ["page"],
            VaryByHeaderNames = [ExtractFilteringRequestExtension.HeaderName])]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Changes(
            [FromServices] FeedContext context,
            [FromQuery] int? page,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameFeedFilter>();
            if (page is null)
                page = filtering.Filter?.Page ?? 1;

            var feedPosition = filtering.Filter?.FeedPosition;

            if (feedPosition.HasValue && filtering.Filter?.Page.HasValue == false)
            {
                page = context.StreetNameFeed
                    .Where(x => x.Position == feedPosition.Value)
                    .Select(x => x.Page)
                    .Distinct()
                    .AsEnumerable()
                    .DefaultIfEmpty(1)
                    .Min();
            }

            var feedItemsEvents = await context
                .StreetNameFeed
                .Where(x => x.Page == page)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return new ChangeFeedResult(jsonContent, feedItemsEvents.Count >= ChangeFeedService.DefaultMaxPageSize);
        }

        /// <summary>
        /// Vraag wijzigingen van een bepaalde gemeente op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{persistentLocalId}/wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangesByPersistentLocalId(
            [FromServices] FeedContext context,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var pagination = (PaginationRequest)Request.ExtractPaginationRequest();

            var feedItemsEvents = await context
                .StreetNameFeed
                .Where(x => x.PersistentLocalId == persistentLocalId)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return Content(jsonContent, AcceptTypes.JsonCloudEventsBatch);
        }

        [HttpGet("posities")]
        [Produces(AcceptTypes.Json)]
        [ProducesResponseType(typeof(FeedPositieResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions(
            [FromServices] LegacyContext legacyContext,
            [FromServices] FeedContext feedContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNamePositionFilter>();
            var response = new FeedPositieResponse();
            if (filtering.ShouldFilter && !filtering.Filter.HasMoreThanOneFilter)
            {
                if (filtering.Filter.Download.HasValue)
                {
                    var businessFeedPosition = await legacyContext
                        .StreetNameSyndication
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .StreetNameFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = businessFeedPosition;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.Sync.HasValue)
                {
                    var changeFeed = await feedContext
                        .StreetNameFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Sync.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = filtering.Filter.Sync.Value;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.ChangeFeedId.HasValue)
                {
                    var feedItem = await feedContext
                        .StreetNameFeed
                        .AsNoTracking()
                        .Where(x => x.Id == filtering.Filter.ChangeFeedId.Value)
                        .Select(x => new { x.Id, x.Page, x.Position })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (feedItem is null)
                        return Ok(response);

                    response.Feed = feedItem.Position;
                    response.WijzigingenFeedPagina = feedItem.Page;
                    response.WijzigingenFeedId = feedItem.Id;
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van straatnamen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var result = await _mediator.Send(new SyndicationRequest(filtering, sorting, pagination), cancellationToken);

            return new ContentResult
            {
                Content = result.Content,
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
