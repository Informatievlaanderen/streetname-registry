namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class RenameStreetNameRequestExamples : IExamplesProvider<RenameStreetNameRequest>
    {
        public RenameStreetNameRequest GetExamples()
        {
            return new RenameStreetNameRequest
            {
                DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/45041"
            };
        }
    }
}
