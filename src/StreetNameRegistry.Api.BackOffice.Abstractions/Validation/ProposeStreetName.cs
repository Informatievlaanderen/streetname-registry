namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class ProposeStreetName
        {
            public static class StreetNameIsMissingALanguage
            {
                public const string Code = "StraatnaamOntbreektOfficieleOfFaciliteitenTaal";
                public const string Message = "In 'Straatnamen' ontbreekt een officiële of faciliteitentaal.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class MunicipalityUnknown
            {
                public const string Code = "StraatnaamGemeenteNietGekendValidatie";
                public static string Message(string gemeenteId) => $"De gemeente '{gemeenteId}' is niet gekend in het gemeenteregister.";

                public static TicketError ToTicketError(string gemeenteId) => new TicketError(Message(gemeenteId), Code);
            }
        }
    }
}
