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
                public const string Code = "StraatnaamHomoniemToevoegingBestaatReedsInGemeente";
                public const string Message = "Binnen deze gemeente bestaat er reeds een straatnaam met status 'voorgesteld' of 'inGebruik' met dezelfde straatnaam en homoniemToevoeging.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class HomonymAdditionMaxCharacterLengthExceeded
            {
                public const string Code = "StraatnaamHomoniemToevoegingMaxlengteValidatie";
                public static string Message(Taal taalCode, int numberOfCharacters) => $"Maximum lengte van een homoniemToevoeging in '{taalCode.ToString().ToLower()}' is 20 tekens. U heeft momenteel {numberOfCharacters} tekens.";

                public static TicketError ToTicketError(Language language, int numberOfCharacters) => new TicketError(Message(language.ToTaal(), numberOfCharacters), Code);
            }

            public static class CannotAddHomonymAddition
            {
                public const string Code = "StraatnaamHomoniemToevoeging";
                public static string Message(Taal taalCode) => $"Er kan geen homoniemToevoeging worden toegevoegd voor taalcode '{taalCode.ToString().ToLower()}'.";

                public static TicketError ToTicketError(Language language) => new TicketError(Message(language.ToTaal()), Code);
            }
        }
    }
}
