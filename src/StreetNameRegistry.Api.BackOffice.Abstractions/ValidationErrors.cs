namespace StreetNameRegistry.Api.BackOffice.Abstractions
{
    public static class ValidationErrorCodes
    {
        public static class Municipality
        {
            public const string MunicipalityStatusNotCurrent = "StraatnaamGemeenteInGebruik";
            public const string MunicipalityHasInvalidStatus = "StraatnaamGemeenteVoorgesteldOfInGebruik";
        }

        public static class StreetName
        {
            public const string StreetNameNotFound = "OnbestaandeStraatnaam";
            public const string StreetNameIsRemoved = "VerwijderdeStraatnaam";
            public const string StreetNameAlreadyExists = "StraatnaamBestaatReedsInGemeente";
            public const string StreetNameCannotBeApproved = "StraatnaamGehistoreerdOfAfgekeurd";
            public const string StreetNameApprovalCannotBeCorrect = "StraatnaamGehistoreerdOfAfgekeurd";
            public const string StreetNameCannotBeCorrected = "StraatnaamGehistoreerdOfAfgekeurd";
            public const string StreetNameCannotBeRejected = "StraatnaamInGebruikOfGehistoreerd";
            public const string StreetNameRejectionCannotBeCorrect = "StraatnaamInGebruikOfGehistoreerd";
            public const string StreetNameCannotBeRetired = "StraatnaamVoorgesteldOfAfgekeurd";
            public const string StreetNameRetirementCannotBeCorrect = "StraatnaamVoorgesteldOfAfgekeurd";
            public const string StreetNameNameLanguageIsNotSupported = "StraatnaamTaalNietInOfficieleOfFaciliteitenTaal";
            public const string StreetNameIsMissingALanguage = "StraatnaamOntbreektOfficieleOfFaciliteitenTaal";
            public const string StreetNameMunicipalityUnknown = "StraatnaamGemeenteNietGekendValidatie";
        }
    }
    public static class ValidationErrorMessages
    {
        public static class Municipality
        {
            public const string MunicipalityStatusNotCurrent = "Deze actie is enkel toegestaan binnen gemeenten met status 'inGebruik'.";
            public const string MunicipalityHasInvalidStatus = "Deze actie is enkel toegestaan binnen gemeenten met status 'voorgesteld' of 'inGebruik'.";
        }

        public static class StreetName
        {
            public const string StreetNameNotFound = "Onbestaande straatnaam.";
            public const string StreetNameIsRemoved = "Verwijderde straatnaam.";
            public const string StreetNameCannotBeApproved = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld'.";
            public const string StreetNameApprovalCannotBeCorrect = "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.";
            public const string StreetNameCannotBeCorrected = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.";
            public const string StreetNameCannotBeRetired = "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.";
            public const string StreetNameRetirementCannotBeCorrect = "Deze actie is enkel toegestaan op straatnamen met status 'gehistoreerd'.";
            public const string StreetNameCannotBeRejected = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld'.";
            public const string StreetNameRejectionCannotBeCorrect = "Deze actie is enkel toegestaan op straatnamen met status 'afgekeurd'.";
            public static string StreetNameAlreadyExists(string name) => $"Straatnaam '{name}' bestaat reeds in de gemeente.";
            public static string StreetNameIdInvalid(int persistentLocalId) => $"De waarde '{persistentLocalId}' is ongeldig.";
            public const string StreetNameHasInvalidStatus = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.";
            public const string StreetNameNameLanguageIsNotSupported = "'Straatnamen' kunnen enkel voorkomen in de officiële of faciliteitentaal van de gemeente.";
            public const string StreetNameIsMissingALanguage = "In 'Straatnamen' ontbreekt een officiële of faciliteitentaal.";
            public static string StreetNameMunicipalityUnknown(string gemeenteId) => $"De gemeente '{gemeenteId}' is niet gekend in het gemeenteregister.";
        }
    }
}
