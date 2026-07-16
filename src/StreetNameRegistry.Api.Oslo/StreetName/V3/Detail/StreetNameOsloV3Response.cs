namespace StreetNameRegistry.Api.Oslo.StreetName.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class StreetNameOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de straatnaam.
        /// </summary>
        [JsonProperty("@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de straatnaam envelop.
        /// </summary>
        [JsonProperty("@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "StraatnaamEnvelop";

        /// <summary>
        /// De details van de straatnaam.
        /// </summary>
        [JsonProperty("data", Order = 2, Required = Required.DisallowNull)]
        public StreetNameDetailOsloV3ResponseData Data { get; set; }

        /// <summary>
        /// De hyperlinks die gerelateerd zijn aan de straatnaam.
        /// </summary>
        [JsonProperty("_links", Order = 99, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StreetNameDetailOsloV3ResponseLinks? Links { get; set; }

        [JsonIgnore]
        internal string? LastEventHash { get; set; }

        public StreetNameOsloV3Response(
            string contextUrlDetail,
            int persistentLocalId,
            StraatnaamStatusValue status,
            StraatnaamToegekendDoorGemeente gemeente,
            DateTimeOffset version,
            string? nameDutch = "",
            string? nameFrench = "",
            string? nameGerman = "",
            string? nameEnglish = "",
            string? homonymAdditionDutch = "",
            string? homonymAdditionFrench = "",
            string? homonymAdditionGerman = "",
            string? homonymAdditionEnglish = "",
            string selfDetailUrl = "",
            string addressLinkUrl = "",
            string? lastEventHash = "")
        {
            Context = contextUrlDetail;
            Data = new StreetNameDetailOsloV3ResponseData(
                persistentLocalId,
                status,
                gemeente,
                version,
                nameDutch,
                nameFrench,
                nameGerman,
                nameEnglish,
                homonymAdditionDutch,
                homonymAdditionFrench,
                homonymAdditionGerman,
                homonymAdditionEnglish);
            Links = new StreetNameDetailOsloV3ResponseLinks(
                new Link
                {
                    Href = new Uri(string.Format(selfDetailUrl, persistentLocalId))
                },
                new Link
                {
                    Href = new Uri(string.Format(addressLinkUrl, persistentLocalId))
                });
            LastEventHash = lastEventHash;
        }
    }

    /// <summary>
    /// De details van de straatnaam.
    /// </summary>
    public class StreetNameDetailOsloV3ResponseData
    {
        /// <summary>
        /// Het linked-data type van de straatnaam.
        /// </summary>
        [JsonProperty("straatnaam", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Straatnaam";

        /// <summary>
        /// De unieke en persistente identificator van de straatnaam (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty("@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [JsonProperty("identificator", Order = 2, Required = Required.DisallowNull)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeente aan dewelke de straatnaam is toegewezen.
        /// </summary>
        [JsonProperty("isToegekendDoor", Order = 3, Required = Required.DisallowNull)]
        public StraatnaamToegekendDoorGemeente Gemeente { get; set; }

        /// <summary>
        /// De straatnaam in verschillende talen.
        /// </summary>
        [JsonProperty("straatnaam", Order = 4, Required = Required.DisallowNull)]
        public List<GeografischeNaam> Straatnamen { get; set; }

        /// <summary>
        /// De homoniem-toevoegingen aan de straatnaam in verschillende talen.
        /// </summary>
        [JsonProperty("homoniemToevoeging", Order = 5, Required = Required.DisallowNull)]
        public List<GeografischeNaam> HomoniemToevoegingen { get; set; }

        /// <summary>
        /// De huidige fase in de levensloop van een straatnaam.
        /// </summary>
        [JsonProperty("status", Order = 6, Required = Required.DisallowNull)]
        public StraatnaamStatus StraatnaamStatus { get; set; }

        public StreetNameDetailOsloV3ResponseData(
            int persistentLocalId,
            StraatnaamStatusValue status,
            StraatnaamToegekendDoorGemeente gemeente,
            DateTimeOffset version,
            string? nameDutch = "",
            string? nameFrench = "",
            string? nameGerman = "",
            string? nameEnglish = "",
            string? homonymAdditionDutch = "",
            string? homonymAdditionFrench = "",
            string? homonymAdditionGerman = "",
            string? homonymAdditionEnglish = "")
        {
            Id = OsloNamespaces.StraatNaam.ToPuri(persistentLocalId.ToString());
            Identificator = new StraatnaamIdentificator(persistentLocalId.ToString(), version);
            StraatnaamStatus = new StraatnaamStatus(status);
            Gemeente = gemeente;

            var straatNamen = new List<GeografischeNaam>
            {
                new GeografischeNaam(nameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(nameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(nameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(nameEnglish ?? string.Empty, Taal.En)
            };

            Straatnamen = straatNamen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();

            var homoniemen = new List<GeografischeNaam>
            {
                new GeografischeNaam(homonymAdditionDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(homonymAdditionFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(homonymAdditionGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(homonymAdditionEnglish ?? string.Empty, Taal.En)
            };

            HomoniemToevoegingen = homoniemen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();
        }
    }

    /// <summary>
    /// De hyperlinks die gerelateerd zijn aan de straatnaam.
    /// </summary>
    public class StreetNameDetailOsloV3ResponseLinks
    {
        [JsonProperty("self", Required = Required.DisallowNull)]
        public Link Self { get; set; }

        [JsonProperty("adressen", NullValueHandling = NullValueHandling.Ignore, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Link? Adressen { get; set; }

        public StreetNameDetailOsloV3ResponseLinks(
            Link self,
            Link? adressen = null)
        {
            Self = self;
            Adressen = adressen;
        }
    }

    public class StreetNameOsloResponseExamples : IExamplesProvider<StreetNameOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public StreetNameOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameOsloV3Response GetExamples()
        {
            var gemeente = new StraatnaamToegekendDoorGemeente
            {
                Id = OsloNamespaces.Gemeente.ToPuri("31005"),
                Detail = string.Format(_responseOptions.GemeenteDetailUrl, "31005"),
                Gemeentenaam = new Gemeentenaam
                {
                    Gemeentenamen = [new GeografischeNaam("Brugge", Taal.Nl)]
                }
            };

            var rnd = new Random();

            return new StreetNameOsloV3Response(
                _responseOptions.ContextUrlDetail,
                rnd.Next(10000, 15000),
                StraatnaamStatusValue.InGebruik,
                gemeente,
                DateTimeOffset.Now.ToExampleOffset(),
                "Baliestraat",
                nameFrench:string.Empty,
                nameGerman:string.Empty,
                nameEnglish:string.Empty,
                homonymAdditionDutch:string.Empty,
                homonymAdditionFrench:string.Empty,
                homonymAdditionGerman:string.Empty,
                homonymAdditionEnglish:string.Empty,
                _responseOptions.DetailUrl,
                _responseOptions.StreetNameDetailAddressesLink);
        }
    }

    public class StreetNameNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public StreetNameNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return new ProblemDetails();
            }

            return new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:streetname:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande straatnaam.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(httpContext, "v3")
            };
        }
    }

    public class StreetNameGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public StreetNameGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return new ProblemDetails();
            }

            return new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:streetname:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderde straatnaam.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(httpContext, "v3")
            };
        }
    }
}
