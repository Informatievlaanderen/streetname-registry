namespace StreetNameRegistry.Api.Legacy.StreetName.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "StraatnaamCollectie", Namespace = "")]
    public class StreetNameBosaResponse
    {
        /// <summary>
        /// De verzameling van straatnamen.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 1)]
        public List<StreetNameBosaItemResponse> Straatnamen { get; set; }

        /// <summary>
        /// Het totaal aantal straatnamen die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 2)]
        //public long TotaalAantal { get; set; }

        public StreetNameBosaResponse()
        {
            Straatnamen = new List<StreetNameBosaItemResponse>();
        }
    }

    [DataContract(Name = "StraatnaamCollectieItem", Namespace = "")]
    public class StreetNameBosaItemResponse
    {
        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// Een lijst van straatnamen, per taal.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 2)]
        public List<Straatnaam> Straatnamen { get; set; }

        /// <summary>
        /// De huidige fase in de levensloop van een straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamStatus", Order = 3)]
        public StraatnaamStatus StraatnaamStatus { get; set; }

        /// <summary>
        /// De identificator van de gemeente.
        /// </summary>
        [DataMember(Name = "GemeenteIdentificator", Order = 4)]
        public GemeenteIdentificator GemeenteIdentificator { get; set; }

        /// <summary>
        /// Een lijst van namen van de gekoppelde gemeente, per taal.
        /// </summary>
        [DataMember(Name = "GemeenteNamen", Order = 5)]
        public List<Gemeentenaam> GemeenteNamen { get; set; }

        public StreetNameBosaItemResponse(
            string id,
            string gemeenteId,
            string naamruimte,
            string gemeenteNaamruimte,
            DateTimeOffset? version,
            string gemeenteVersion,
            StraatnaamStatus status,
            IEnumerable<GeografischeNaam> straatnamen,
            IEnumerable<GeografischeNaam> gemeenteNamen)
        {
            Identificator = new StraatnaamIdentificator(naamruimte, id, version);
            GemeenteIdentificator = new GemeenteIdentificator(gemeenteNaamruimte, gemeenteId, gemeenteVersion);
            Straatnamen = straatnamen.Select(g => new Straatnaam(g)).ToList();
            GemeenteNamen = (gemeenteNamen?.Select(g => new Gemeentenaam(g)) ?? Enumerable.Empty<Gemeentenaam>()).ToList();
            StraatnaamStatus = status;
        }
    }

    public class StreetNameBosaResponseExamples : IExamplesProvider<StreetNameBosaResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameBosaResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameBosaResponse GetExamples()
        {
            var streetNameExamples = new List<StreetNameBosaItemResponse>
            {
                new StreetNameBosaItemResponse(
                    "26839",
                    "23038",
                    _responseOptions.Naamruimte,
                    _responseOptions.GemeenteNaamruimte,
                    DateTimeOffset.Now.ToExampleOffset(),
                    new Rfc3339SerializableDateTimeOffset(new DateTimeOffset(1830, 1, 1, 0, 0, 0, TimeSpan.FromHours(1))).ToString(),
                    StraatnaamStatus.InGebruik,
                    new[]
                    {
                        new GeografischeNaam("Schoolstraat", Taal.NL)
                    },
                    new[]
                    {
                        new GeografischeNaam("Kampenhout", Taal.NL),
                    }),

                new StreetNameBosaItemResponse(
                    "28012",
                    "28316",
                    _responseOptions.Naamruimte,
                    _responseOptions.GemeenteNaamruimte,
                    DateTimeOffset.Now.ToExampleOffset(),
                    new Rfc3339SerializableDateTimeOffset(new DateTimeOffset(1830, 1, 1, 0, 0, 0, TimeSpan.FromHours(1))).ToString(),
                    StraatnaamStatus.InGebruik,
                    new[]
                    {
                        new GeografischeNaam("Schoolstraat", Taal.NL)
                    },
                    new[]
                    {
                        new GeografischeNaam("Opwijk", Taal.NL),
                    }),
            };

            return new StreetNameBosaResponse
            {
                Straatnamen = streetNameExamples,
            };
        }
    }
}
