namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "StraatnaamCollectieOslo", Namespace = "")]
    public class StreetNameListOsloResponse
    {
        /// <summary>
        /// De linked-data context van de straatnaam.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// De verzameling van straatnamen.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<StreetNameListOsloItemResponse> Straatnamen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri? Volgende { get; set; }

        [JsonIgnore]
        internal SortingHeader Sorting { get; set; }

        [JsonIgnore]
        internal PaginationInfo Pagination { get; set; }
    }

    [DataContract(Name = "StraatnaamCollectieItem", Namespace = "")]
    public class StreetNameListOsloItemResponse
    {
        /// <summary>
        /// Het linked-data type van de straatnaam.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Straatnaam";

        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de straatnaam weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De straatnaam in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Straatnaam Straatnaam { get; set; }

        /// <summary>
        /// De homoniemtoevoeging in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        /// <summary>
        /// De huidige fase in de levensloop van een straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamStatus", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public StraatnaamStatus StraatnaamStatus { get; set; }

        public StreetNameListOsloItemResponse(
            int? id,
            string naamruimte,
            string detail,
            GeografischeNaam geografischeNaam,
            GeografischeNaam? homoniemToevoeging,
            StraatnaamStatus status,
            DateTimeOffset? version)
        {
            Identificator = new StraatnaamIdentificator(naamruimte, id?.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Straatnaam = new Straatnaam(geografischeNaam);
            StraatnaamStatus = status;

            if (homoniemToevoeging != null)
            {
                HomoniemToevoeging = new HomoniemToevoeging(homoniemToevoeging);
            }
        }
    }

    public class StreetNameListOsloResponseExamples : IExamplesProvider<StreetNameListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameListOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameListOsloResponse GetExamples()
        {
            var streetNameSamples = new List<StreetNameListOsloItemResponse>
                {
                    new StreetNameListOsloItemResponse(
                        1000,
                        _responseOptions.Naamruimte,
                        _responseOptions.DetailUrl,
                        new GeografischeNaam("Kerkstraat", Taal.NL),
                        null,
                        StraatnaamStatus.InGebruik,
                        DateTimeOffset.Now.ToExampleOffset()),

                    new StreetNameListOsloItemResponse(
                        1001,
                        _responseOptions.Naamruimte,
                        _responseOptions.DetailUrl,
                        new GeografischeNaam("Wetstraat", Taal.NL),
                        new GeografischeNaam("BR", Taal.NL),
                        StraatnaamStatus.Voorgesteld,
                        DateTimeOffset.Now.ToExampleOffset())
                };

            return new StreetNameListOsloResponse
            {
                Straatnamen = streetNameSamples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
