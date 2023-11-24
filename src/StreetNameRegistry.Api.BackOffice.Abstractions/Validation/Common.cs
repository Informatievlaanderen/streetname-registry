namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class MunicipalityStatusNotCurrent
            {
                public const string Code = "StraatnaamGemeenteInGebruik";
                public const string Message = "Deze actie is enkel toegestaan binnen gemeenten met status 'inGebruik'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class MunicipalityHasInvalidStatus
            {
                public const string Code = "StraatnaamGemeenteVoorgesteldOfInGebruik";
                public const string Message = "Deze actie is enkel toegestaan binnen gemeenten met status 'voorgesteld' of 'inGebruik'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class StreetNameInvalid
            {
                public const string Code = "StraatnaamNietGekendValidatie";
                public static string Message(string streetNamePuri) => $"De straatnaam '{streetNamePuri}' is niet gekend in het straatnaamregister.";
            }

            public static class StreetNameNotFound
            {
                public const string Code = "OnbestaandeStraatnaam";
                public const string Message = "Onbestaande straatnaam.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class StreetNameIsRemoved
            {
                public const string Code = "VerwijderdeStraatnaam";
                public const string Message = "Verwijderde straatnaam.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class StreetNameAlreadyExists
            {
                public const string Code = "StraatnaamBestaatReedsInGemeente";
                public static string Message(string name) => $"Straatnaam '{name}' bestaat reeds in de gemeente.";

                public static TicketError ToTicketError(string name) => new TicketError(Message(name), Code);
            }

            public static class StreetNameNameLanguageIsNotSupported
            {
                public const string Code = "StraatnaamTaalNietInOfficieleOfFaciliteitenTaal";
                public const string Message = "'Straatnamen' kunnen enkel voorkomen in de officiële of faciliteitentaal van de gemeente.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
