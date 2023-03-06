namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Swashbuckle.AspNetCore.Filters;

    public class CorrectStreetNameHomonymAdditionsRequestExamples : IExamplesProvider<CorrectStreetNameHomonymAdditionsRequest>
    {
        public CorrectStreetNameHomonymAdditionsRequest GetExamples()
        {
            return new CorrectStreetNameHomonymAdditionsRequest
            {
                HomoniemToevoegingen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "HO" },
                    { Taal.FR, "HO" }
                }
            };
        }
    }
}
