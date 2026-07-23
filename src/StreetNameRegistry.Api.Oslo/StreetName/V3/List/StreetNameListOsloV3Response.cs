namespace StreetNameRegistry.Api.Oslo.StreetName.V3.List
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class StreetNameListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de straatnaam.
        /// </summary>
        [JsonProperty("@context", Order = 0, Required = Required.DisallowNull)]
        public required string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de straatnamen envelop.
        /// </summary>
        [JsonProperty("@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "StraatnamenEnvelop";

        /// <summary>
        /// De verzameling van straatnamen.
        /// </summary>
        [JsonProperty("data", Order = 2, Required = Required.DisallowNull)]
        public required List<StreetNameListItemOsloV3Response> Straatnamen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [JsonProperty("volgende", Order = 3, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri? Volgende { get; set; }

        [JsonIgnore]
        internal SortingHeader Sorting { get; set; }

        [JsonIgnore]
        internal PaginationInfo Pagination { get; set; }
    }

    public class StreetNameListItemOsloV3Response
    {
        /// <summary>
        /// Het linked-data type van de straatnaam.
        /// </summary>
        [JsonProperty("@type", Order = 0, Required = Required.DisallowNull)]
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
        /// De URL die de details van de meest recente versie van de straatnaam weergeeft.
        /// </summary>
        [JsonProperty("detail", Order = 3, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De straatnaam in verschillende talen.
        /// </summary>
        [JsonProperty("straatnaam", Order = 4, Required = Required.DisallowNull)]
        public List<GeografischeNaam> Straatnamen { get; set; }

        /// <summary>
        /// De homoniem-toevoegingen aan de straatnaam in verschillende talen.
        /// </summary>
        [JsonProperty("homoniemToevoeging", Order = 5, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<GeografischeNaam> HomoniemToevoegingen { get; set; }

        /// <summary>
        /// De huidige fase in de levensloop van een straatnaam.
        /// </summary>
        [JsonProperty("status", Order = 6, Required = Required.DisallowNull)]
        public StraatnaamStatus StraatnaamStatus { get; set; }

        public StreetNameListItemOsloV3Response(
            int id,
            string detail,
            IEnumerable<GeografischeNaam> geografischeNamen,
            IEnumerable<GeografischeNaam>? homoniemToevoegingen,
            StraatnaamStatusValue status,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.StraatNaam.ToPuri(id.ToString());
            Identificator = new StraatnaamIdentificator(id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Straatnamen = new List<GeografischeNaam>(geografischeNamen);
            StraatnaamStatus = new StraatnaamStatus(status);

            if (homoniemToevoegingen != null)            
                HomoniemToevoegingen = new List<GeografischeNaam>(homoniemToevoegingen);
        }
    }

    public class StreetNameListOsloResponseExamples : IExamplesProvider<StreetNameListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public StreetNameListOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameListOsloV3Response GetExamples()
        {
            var streetNameSamples = new List<StreetNameListItemOsloV3Response>
                {
                    new StreetNameListItemOsloV3Response(
                        1000,
                        _responseOptions.DetailUrl,
                        [new GeografischeNaam("Kerkstraat", Taal.Nl)],
                        null,
                        StraatnaamStatusValue.InGebruik,
                        DateTimeOffset.Now.ToExampleOffset()),

                    new StreetNameListItemOsloV3Response(
                        1001,
                        _responseOptions.DetailUrl,
                        [new GeografischeNaam("Wetstraat", Taal.Nl)],
                        [new GeografischeNaam("BR", Taal.Nl)],
                        StraatnaamStatusValue.Voorgesteld,
                        DateTimeOffset.Now.ToExampleOffset())
                };

            return new StreetNameListOsloV3Response
            {
                Straatnamen = streetNameSamples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
