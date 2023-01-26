namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class ProposeStreetName
        {
            public static class StreetNameIsMissingALanguage
            {
                public const string Code = "StraatnaamOntbreektOfficieleOfFaciliteitenTaal";
                public const string Message = "In 'Straatnamen' ontbreekt een officiële of faciliteitentaal.";
            }

            public static class MunicipalityUnknown
            {
                public const string StreetNameMunicipalityUnknown = "StraatnaamGemeenteNietGekendValidatie";
                public static string Message(string gemeenteId) => $"De gemeente '{gemeenteId}' is niet gekend in het gemeenteregister.";
            }
        }
    }
}
