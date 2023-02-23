namespace StreetNameRegistry.Api.BackOffice.Abstractions.Convertors
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Municipality;

    public static class TaalMapper
    {
        public static Language ToLanguage(this Taal taal)
        {
            return taal switch
            {
                Taal.NL => Language.Dutch,
                Taal.FR => Language.French,
                Taal.DE => Language.German,
                Taal.EN => Language.English,
                _ => throw new ArgumentOutOfRangeException(nameof(taal), taal, $"Non existing language '{taal}'.")
            };
        }

        public static Taal ToTaal(this Language language)
        {
            return language switch
            {
                Language.Dutch => Taal.NL,
                Language.French => Taal.FR,
                Language.German => Taal.DE,
                Language.English => Taal.EN,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, $"Non existing language '{language}'.")
            };
        }
    }

    public static class IdentifierMappings
    {
        public static readonly Func<string, string> MunicipalityNisCode = s => s;
    }
}
