namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Swashbuckle.AspNetCore.Filters;

public class StreetNameCorrectNamesRequestExamples : IExamplesProvider<CorrectStreetNameNamesRequest>
{
    public CorrectStreetNameNamesRequest GetExamples()
    {
        return new CorrectStreetNameNamesRequest
        {
            Straatnamen = new Dictionary<Taal, string>
            {
                {Taal.NL, "Rodekruisstraat"},
                {Taal.FR, "Rue de la Croix-Rouge"}
            }
        };
    }
}
