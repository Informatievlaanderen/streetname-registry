namespace StreetNameRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public enum Language
    {
        Dutch = 0,
        French = 1,
        German = 2,
        English = 3
    }

    public static class LanguageHelpers
    {
        public static Language ToLanguage(this CrabLanguage language)
        {
            switch (language)
            {
                case CrabLanguage.Dutch:
                    return Language.Dutch;

                case CrabLanguage.French:
                    return Language.French;

                case CrabLanguage.German:
                    return Language.German;

                case CrabLanguage.English:
                    return Language.English;

                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, $"Non existing language '{language}'.");
            }
        }

        public static Municipality.Language ToMunicipalityLanguage(this Language? language)
        {
            if (!language.HasValue)
                throw new InvalidOperationException("Cannot convert null StreetName.Language.");

            return Map(language.Value);
        }

        public static Municipality.Language? ToNullableMunicipalityLanguage(this Language? language)
        {
            if (!language.HasValue)
                return null;

            return Map(language.Value);
        }

        private static Municipality.Language Map(Language language)
        {
            return language switch
            {
                Language.Dutch => Municipality.Language.Dutch,
                Language.English => Municipality.Language.English,
                Language.French => Municipality.Language.French,
                Language.German => Municipality.Language.German,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, $"Non existing language '{language}'.")
            };
        }
    }
}
