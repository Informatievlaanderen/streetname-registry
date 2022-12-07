namespace StreetNameRegistry.Api.Legacy.StreetName.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class TotalCountResponseExample : IExamplesProvider<TotaalAantalResponse>
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
