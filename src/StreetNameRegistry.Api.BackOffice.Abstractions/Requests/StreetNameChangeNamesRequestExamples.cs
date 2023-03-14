namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;

    public class StreetNameChangeNamesRequestExamples : IExamplesProvider<ChangeStreetNameNamesRequest>
    {
        public ChangeStreetNameNamesRequest GetExamples()
        {
            return new ChangeStreetNameNamesRequest
            {
                Straatnamen = new Dictionary<Taal, string>
                {
                    {Taal.NL, "Rodekruisstraat"},
                    {Taal.FR, "Rue de la Croix-Rouge"}
                }
            };
        }
    }
}
