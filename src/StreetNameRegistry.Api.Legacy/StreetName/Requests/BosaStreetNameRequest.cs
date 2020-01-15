namespace StreetNameRegistry.Api.Legacy.StreetName.Requests
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Swashbuckle.AspNetCore.Filters;

    public class BosaStreetNameRequest
    {
        public ZoekGeografischeNaam Straatnaam { get; set; }
        public ZoekIdentifier StraatnaamCode { get; set; }
        public ZoekIdentifier GemeenteCode { get; set; }
        public StraatnaamStatus? StraatnaamStatus { get; set; }
    }

    public class StreetNameBosaRequestExamples : IExamplesProvider<BosaStreetNameRequest>
    {
        public BosaStreetNameRequest GetExamples()
            => new BosaStreetNameRequest
            {
                Straatnaam = new ZoekGeografischeNaam
                {
                    Spelling = "school",
                    Taal = Taal.NL,
                    SearchType = BosaSearchType.Bevat
                },
                StraatnaamCode = new ZoekIdentifier
                {
                    ObjectId = "2",
                    VersieId = DateTimeOffset.Now
                },
                StraatnaamStatus = StraatnaamStatus.InGebruik,
                GemeenteCode = new ZoekIdentifier
                {
                    ObjectId = "230",
                    VersieId = DateTimeOffset.Now
                }
            };
    }
}
