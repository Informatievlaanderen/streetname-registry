namespace StreetNameRegistry.Api.Legacy.StreetName
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameList;
    using Projections.Syndication;
    using Query;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("straatnamen")]
    [ApiExplorerSettings(GroupName = "Straatnamen")]
    public class StreetNameController : ApiController
    {
        /// <summary>
        /// Vraag een straatnaam op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="osloId">De Oslo identificator van de straatnaam.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de straatnaam gevonden is.</response>
        /// <response code="404">Als de straatnaam niet gevonden kan worden.</response>
        /// <response code="410">Als de straatnaam verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{osloId}")]
        [ProducesResponseType(typeof(StreetNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(StreetNameNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(StreetNameGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext legacyContext,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int osloId,
            CancellationToken cancellationToken = default)
        {
            return Ok(await
                new StreetNameDetailQuery(legacyContext, syndicationContext, responseOptions)
                    .FilterAsync(osloId, cancellationToken));
        }

        /// <summary>
        /// Vraag een specifieke versie van een straatnaam op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="osloId">De Oslo identificator van de straatnaam.</param>
        /// <param name="versie">De specifieke versie van de straatnaam.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de straatnaam gevonden is.</response>
        /// <response code="404">Als de straatnaam niet gevonden kan worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{osloId}/versies/{versie}")]
        [ProducesResponseType(typeof(StreetNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(StreetNameNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext legacyContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int osloId,
            [FromRoute] DateTimeOffset versie,
            CancellationToken cancellationToken = default)
        {
            var streetName = await legacyContext
                .StreetNameVersions
                .AsNoTracking()
                .Where(x => !x.Removed)
                .SingleOrDefaultAsync(item => item.OsloId == osloId && item.VersionTimestampAsDateTimeOffset == versie, cancellationToken);

            if (streetName == null)
                throw new ApiException("Onbestaande straatnaam.", StatusCodes.Status404NotFound);

            var gemeente = new StraatnaamDetailGemeente
            {
                ObjectId = streetName.NisCode,
                Detail = string.Format(responseOptions.Value.GemeenteDetailUrl, streetName.NisCode),
                // todo: get the name for this nisCode's municipality.
                // Not feasible yet, at least not yet without calling the Municipality Api.
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam(null, Taal.NL))
            };

            return Ok(
                new StreetNameResponse(
                    responseOptions.Value.Naamruimte,
                    osloId,
                    streetName.Status.ConvertFromStreetNameStatus(),
                    gemeente,
                    streetName.VersionTimestampAsDateTimeOffset.Value,
                    streetName.NameDutch,
                    streetName.NameFrench,
                    streetName.NameGerman,
                    streetName.NameEnglish,
                    streetName.HomonymAdditionDutch,
                    streetName.HomonymAdditionFrench,
                    streetName.HomonymAdditionGerman,
                    streetName.HomonymAdditionEnglish));
        }

        /// <summary>
        /// Vraag een lijst met straatnamen op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="taal">Gewenste taal van de respons.</param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met straatnamen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<StreetNameListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] LegacyContext legacyContext,
            [FromServices] IHostingEnvironment hostingEnvironment,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            Taal? taal,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedStreetNames = new StreetNameListQuery(legacyContext, syndicationContext)
                .Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedStreetNames.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            taal = taal ?? Taal.NL;

            return Ok(
                new StreetNameListResponse
                {
                    Straatnamen = await pagedStreetNames
                        .Items
                        .Select(m => new StreetNameListItemResponse(
                            m.OsloId,
                            responseOptions.Value.Naamruimte,
                            responseOptions.Value.DetailUrl,
                            GetGeografischeNaamByTaal(m, taal.Value),
                            GetHomoniemToevoegingByTaal(m, taal.Value),
                            m.VersionTimestamp.ToBelgianDateTimeOffset()))
                        .ToListAsync(cancellationToken),
                    TotaalAantal = pagedStreetNames.PaginationInfo.TotalItems,
                    Volgende = BuildVolgendeUri(pagedStreetNames.PaginationInfo, responseOptions.Value.VolgendeUrl)
                });
        }

        /// <summary>
        /// Vraag een lijst met versies van een straatnaam op.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="responseOptions"></param>
        /// <param name="osloId">De Oslo identifier van de straatnaam.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met versies van de straatnaam gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{osloId}/versies")]
        [ProducesResponseType(typeof(List<StreetNameVersionListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameVersionListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext legacyContext,
            [FromServices] IHostingEnvironment hostingEnvironment,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int osloId,
            CancellationToken cancellationToken = default)
        {
            var streetNameVersions =
                await legacyContext
                    .StreetNameVersions
                    .AsNoTracking()
                    .Where(p => !p.Removed && p.OsloId == osloId)
                    .ToListAsync(cancellationToken);

            if (!streetNameVersions.Any())
                throw new ApiException("Onbestaande straatnaam.", StatusCodes.Status404NotFound);

            return Ok(
                new StreetNameVersionListResponse
                {
                    StraatnaamVersies = streetNameVersions
                        .Select(m => new StreetNameVersionResponse(
                            m.VersionTimestampAsDateTimeOffset,
                            responseOptions.Value.DetailUrl,
                            osloId))
                        .ToList()
                });
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van straatnamen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="embed">Om volledige objecten terug te krijgen, zet embed op true.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            bool embed = false,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<StreetNameSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedStreetNames = new StreetNameSyndicationQuery(context, embed).Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedStreetNames.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            return new ContentResult
            {
                Content = await BuildAtomFeed(pagedStreetNames, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Zoek naar straatnamen in het Vlaams Gewest in het BOSA formaat.
        /// </summary>
        /// <param name="legacyContext"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="request">De request in BOSA formaat.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("bosa")]
        [ProducesResponseType(typeof(StreetNameBosaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(BosaStreetNameRequest), typeof(StreetNameBosaRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(StreetNameBosaResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Post(
            [FromServices] LegacyContext legacyContext,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromBody] BosaStreetNameRequest request,
            CancellationToken cancellationToken = default)
        {
            var filter = new StreetNameNameFilter(request);
            var streetNames = (await
                new StreetNameBosaQuery(
                    legacyContext,
                    syndicationContext,
                    responseOptions)
                .FilterAsync(filter, cancellationToken))
                .ToList();

            return Ok(new StreetNameBosaResponse
            {
                Straatnamen = streetNames,
                TotaalAantal = streetNames.Count
            });
        }

        private static GeografischeNaam GetGeografischeNaamByTaal(StreetNameListItem item, Taal taal)
        {
            switch (taal)
            {
                case Taal.FR when !string.IsNullOrEmpty(item.NameFrench):
                    return new GeografischeNaam(
                        item.NameFrench,
                        Taal.FR);

                case Taal.DE when !string.IsNullOrEmpty(item.NameGerman):
                    return new GeografischeNaam(
                        item.NameGerman,
                        Taal.DE);

                case Taal.EN when !string.IsNullOrEmpty(item.NameEnglish):
                    return new GeografischeNaam(
                        item.NameEnglish,
                        Taal.EN);

                default:
                case Taal.NL:
                    return new GeografischeNaam(
                        item.NameDutch,
                        Taal.NL);
            }
        }

        private static GeografischeNaam GetHomoniemToevoegingByTaal(StreetNameListItem item, Taal taal)
        {
            switch (taal)
            {
                case Taal.NL when !string.IsNullOrEmpty(item.HomonymAdditionDutch):
                    return new GeografischeNaam(
                        item.HomonymAdditionDutch,
                        Taal.NL);

                case Taal.FR when !string.IsNullOrEmpty(item.HomonymAdditionFrench):
                    return new GeografischeNaam(
                        item.HomonymAdditionFrench,
                        Taal.FR);

                case Taal.DE when !string.IsNullOrEmpty(item.HomonymAdditionGerman):
                    return new GeografischeNaam(
                        item.NameGerman,
                        Taal.DE);

                case Taal.EN when !string.IsNullOrEmpty(item.HomonymAdditionEnglish):
                    return new GeografischeNaam(
                        item.NameEnglish,
                        Taal.EN);

                default:
                    return null;
            }
        }

        private static async Task<string> BuildAtomFeed(
            PagedQueryable<StreetNameSyndicationQueryResult> pagedStreetNames,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");

                await writer.WriteDefaultMetadata(
                    syndicationConfiguration["Id"],
                    syndicationConfiguration["Title"],
                    Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    new Uri(syndicationConfiguration["Self"]),
                    syndicationConfiguration.GetSection("Related").GetChildren().Select(c => c.Value).ToArray());

                var nextUri = BuildVolgendeUri(pagedStreetNames.PaginationInfo, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, GrArAtomLinkTypes.Next));

                foreach (var streetName in pagedStreetNames.Items)
                    await writer.WriteStreetName(responseOptions, formatter, syndicationConfiguration["Category"], streetName);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildVolgendeUri(PaginationInfo paginationInfo, string volgendeUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return offset + limit < paginationInfo.TotalItems
                ? new Uri(string.Format(volgendeUrlBase, offset + limit, limit))
                : null;
        }
    }
}
