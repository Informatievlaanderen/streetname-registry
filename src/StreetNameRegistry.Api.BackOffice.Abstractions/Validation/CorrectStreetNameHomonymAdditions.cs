namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Convertors;
    using Municipality;
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameHomonymAdditions
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class StreetNameNameWithHomonymAdditionAlreadyExists
            {
                public const string Code = "StraatnaamHomoniemBestaatReedsInGemeente";
                public const string Message = "Binnen deze gemeente bestaat er reeds een niet-gehistoreerd straatnaamobject met dezelfde straatnaam en homoniemtoevoeging.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class HomonymAdditionMaxCharacterLengthExceeded
            {
                public const string Code = "StraatnaamHomoniemMaxlengteValidatie";
                public static string Message(Taal taalCode) => $"Homoniemtoevoeging mag maximaal 20 karakters lang zijn.";

                public static TicketError ToTicketError(Language language) => new TicketError(Message(language.ToTaal()), Code);
            }

            public static class CannotAddHomonymAddition
            {
                public const string Code = "StraatnaamHomoniemToevoeging";
                public static string Message(Taal taalCode) => $"Er kan geen homoniemToevoeging worden toegevoegd voor taalcode {taalCode}.";

                public static TicketError ToTicketError(Language language) => new TicketError(Message(language.ToTaal()), Code);
            }
        }
    }
}
