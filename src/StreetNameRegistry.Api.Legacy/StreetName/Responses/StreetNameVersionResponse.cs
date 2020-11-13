namespace StreetNameRegistry.Api.Legacy.StreetName.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract]
    public class StreetNameVersionListResponse
    {
        /// <summary>
        /// De verzameling van versies van een straatnaam.
        /// </summary>
        public List<StreetNameVersionResponse> StraatnaamVersies { get; set; }
    }

    [DataContract]
    public class StreetNameVersionResponse
    {
        /// <summary>
        /// Het tijdstip van de creatie van deze versie.
        /// </summary>
        public DateTimeOffset? Versie { get; set; }

        /// <summary>
        /// De URL naar het detail met deze specifieke versie van dit object.
        /// </summary>
        public Uri VersieDetail { get; set; }

        public StreetNameVersionResponse(DateTimeOffset? versie, string detailUrl, int objectId)
        {
            Versie = versie;
            VersieDetail = new Uri($"{string.Format(detailUrl, objectId)}/versies/{versie}");
        }
    }

    public class StreetNameVersionListResponseExamples : IExamplesProvider<StreetNameVersionListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameVersionListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public StreetNameVersionListResponse GetExamples()
        {
            var rnd = new Random();
            var objectId = rnd.Next(10000, 15000);

            var responseVersion0 = new StreetNameVersionResponse(
                DateTime.Now.AddDays(-20),
                _responseOptions.DetailUrl,
                objectId);

            var responseVersion1 = new StreetNameVersionResponse(
                DateTime.Now.AddDays(-50),
                _responseOptions.DetailUrl,
                objectId);

            return new StreetNameVersionListResponse
            {
                StraatnaamVersies = new List<StreetNameVersionResponse>
                {
                    responseVersion0,
                    responseVersion1
                }
            };
        }
    }
}
