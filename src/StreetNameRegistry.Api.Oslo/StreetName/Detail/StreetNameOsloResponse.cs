namespace StreetNameRegistry.Api.Oslo.StreetName.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "StraatnaamDetailOslo", Namespace = "")]
    public class StreetNameOsloResponse
    {
        /// <summary>
        /// De linked-data context van de straatnaam.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de straatnaam.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Straatnaam";

        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeente aan dewelke de straatnaam is toegewezen.
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public StraatnaamDetailGemeente Gemeente { get; set; }

        /// <summary>
        /// De straatnaam in verschillende talen.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GeografischeNaam> Straatnamen { get; set; }

        /// <summary>
        /// De homoniem-toevoegingen aan de straatnaam in verschillende talen.
        /// </summary>
        [DataMember(Name = "HomoniemToevoegingen", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GeografischeNaam> HomoniemToevoegingen { get; set; }

        /// <summary>
        /// De huidige fase in de levensloop van een straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamStatus", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public StraatnaamStatus StraatnaamStatus { get; set; }

        [JsonIgnore]
        internal string? LastEventHash { get; set; }

        public StreetNameOsloResponse(
            string naamruimte,
            string contextUrlDetail,
            int persistentLocalId,
            StraatnaamStatus status,
            StraatnaamDetailGemeente gemeente,
            DateTimeOffset version,
            string? nameDutch = "",
            string? nameFrench = "",
            string? nameGerman = "",
            string? nameEnglish = "",
            string? homonymAdditionDutch = "",
            string? homonymAdditionFrench = "",
            string? homonymAdditionGerman = "",
            string? homonymAdditionEnglish = "",
            string? lastEventHash = "")
        {
            Context = contextUrlDetail;
            Identificator = new StraatnaamIdentificator(naamruimte, persistentLocalId.ToString(), version);
            StraatnaamStatus = status;
            Gemeente = gemeente;
            LastEventHash = lastEventHash;

            var straatNamen = new List<GeografischeNaam>
            {
                new GeografischeNaam(nameDutch ?? string.Empty, Taal.NL),
                new GeografischeNaam(nameFrench ?? string.Empty, Taal.FR),
                new GeografischeNaam(nameGerman ?? string.Empty, Taal.DE),
                new GeografischeNaam(nameEnglish ?? string.Empty, Taal.EN)
            };

            Straatnamen = straatNamen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();

            var homoniemen = new List<GeografischeNaam>
            {
                new GeografischeNaam(homonymAdditionDutch ?? string.Empty, Taal.NL),
                new GeografischeNaam(homonymAdditionFrench ?? string.Empty, Taal.FR),
                new GeografischeNaam(homonymAdditionGerman ?? string.Empty, Taal.DE),
                new GeografischeNaam(homonymAdditionEnglish ?? string.Empty, Taal.EN)
            };

            HomoniemToevoegingen = homoniemen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();
        }
    }

    public class StreetNameOsloResponseExamples : IExamplesProvider<StreetNameOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameOsloResponse GetExamples()
        {
            var gemeente = new StraatnaamDetailGemeente
            {
                ObjectId = "31005",
                Detail = string.Format(_responseOptions.GemeenteDetailUrl, "31005"),
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Brugge", Taal.NL))
            };

            var rnd = new Random();

            return new StreetNameOsloResponse(
                _responseOptions.Naamruimte,
                _responseOptions.ContextUrlDetail,
                rnd.Next(10000, 15000),
                StraatnaamStatus.InGebruik,
                gemeente,
                DateTimeOffset.Now.ToExampleOffset(),
                "Baliestraat",
                nameFrench:string.Empty,
                nameGerman:string.Empty,
                nameEnglish:string.Empty,
                homonymAdditionDutch:string.Empty,
                homonymAdditionFrench:string.Empty,
                homonymAdditionGerman:string.Empty,
                homonymAdditionEnglish:string.Empty);
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
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:streetname:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande straatnaam.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!)
            };
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
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:streetname:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderde straatnaam.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!)
            };
    }
}
