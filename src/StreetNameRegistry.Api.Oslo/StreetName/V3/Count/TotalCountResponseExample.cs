namespace StreetNameRegistry.Api.Oslo.StreetName.V3.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Swashbuckle.AspNetCore.Filters;

    public class TotalCountResponseExample : IExamplesProvider<TotaalAantalResponse>
    {
        public TotaalAantalResponse GetExamples()
        {
            return new TotaalAantalResponse
            {
                Aantal = 574512
            };
        }
    }
}
