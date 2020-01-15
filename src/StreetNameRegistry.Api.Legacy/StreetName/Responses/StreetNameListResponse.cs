namespace StreetNameRegistry.Api.Legacy.StreetName.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "StraatnaamCollectie", Namespace = "")]
    public class StreetNameListResponse
    {
        /// <summary>
        /// De verzameling van straatnamen.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 1)]
        public List<StreetNameListItemResponse> Straatnamen { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        [DataMember(Name = "TotaalAantal", Order = 2)]
        public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "StraatnaamCollectieItem", Namespace = "")]
    public class StreetNameListItemResponse
    {
        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkele straatnaam leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De straatnaam in het Nederlands.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 3)]
        public Straatnaam Straatnaam { get; set; }

        /// <summary>
        /// De homoniemtoevoeging in het Nederlands.
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 4)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        public StreetNameListItemResponse(
            int? id,
            string naamruimte,
            string detail,
            GeografischeNaam geografischeNaam,
            GeografischeNaam homoniemToevoeging,
            DateTimeOffset? version)
        {
            Identificator = new StraatnaamIdentificator(naamruimte, id?.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Straatnaam = new Straatnaam(geografischeNaam);

            if (homoniemToevoeging != null)
                HomoniemToevoeging = new HomoniemToevoeging(homoniemToevoeging);
        }
    }

    public class StreetNameListResponseExamples : IExamplesProvider<StreetNameListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
         => _responseOptions = responseOptionsProvider.Value;

        public StreetNameListResponse GetExamples()
        {
            var rnd = new Random();

            var streetNameSamples = new List<StreetNameListItemResponse>
                {
                    new StreetNameListItemResponse(
                        rnd.Next(10000, 150000),
                        _responseOptions.Naamruimte,
                        _responseOptions.DetailUrl,
                        new GeografischeNaam("Kerkstraat", Taal.NL),
                        null,
                        DateTimeOffset.Now.LocalDateTime),

                    new StreetNameListItemResponse(
                        rnd.Next(10000, 150000),
                        _responseOptions.Naamruimte,
                        _responseOptions.DetailUrl,
                        new GeografischeNaam("Wetstraat", Taal.NL),
                        null,
                        DateTimeOffset.Now.LocalDateTime)
                };

            return new StreetNameListResponse
            {
                Straatnamen = streetNameSamples,
                TotaalAantal = 2,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
