namespace StreetNameRegistry.Projections.Integration.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Municipality;
    using Language = StreetName.Language;

    public static class StreetNameMapper
    {
        public static string Map(this StreetNameStatus status)
        {
            switch (status)
            {
                case StreetNameStatus.Proposed:
                    return StraatnaamStatus.Voorgesteld.ToString();
                case StreetNameStatus.Current:
                    return StraatnaamStatus.InGebruik.ToString();
                case StreetNameStatus.Retired:
                    return StraatnaamStatus.Gehistoreerd.ToString();
                case StreetNameStatus.Rejected:
                    return StraatnaamStatus.Afgekeurd.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static void UpdateNameByLanguage(this StreetNameVersion item, Language? language, string value)
        {
            switch (language)
            {
                case Language.Dutch:
                    item.NameDutch = value;
                    break;
                case Language.French:
                    item.NameFrench = value;
                    break;
                case Language.German:
                    item.NameGerman = value;
                    break;
                case Language.English:
                    item.NameEnglish = value;
                    break;
                default: throw new InvalidOperationException($"Name language '{language}' has no mapping.");
            }
        }

        public static void UpdateHomonymAdditionByLanguage(this StreetNameVersion item, Language? language, string value)
        {
            switch (language)
            {
                case Language.Dutch:
                    item.HomonymAdditionDutch = value;
                    break;
                case Language.French:
                    item.HomonymAdditionFrench = value;
                    break;
                case Language.German:
                    item.HomonymAdditionGerman = value;
                    break;
                case Language.English:
                    item.HomonymAdditionEnglish = value;
                    break;
                default: throw new InvalidOperationException($"HomonymAddition language '{language}' has no mapping.");
            }
        }

        public static void UpdateNameByLanguage(this StreetNameVersion item, StreetNameRegistry.Municipality.Language language, string value)
        {
            switch (language)
            {
                case StreetNameRegistry.Municipality.Language.Dutch:
                    item.NameDutch = value;
                    break;
                case StreetNameRegistry.Municipality.Language.French:
                    item.NameFrench = value;
                    break;
                case StreetNameRegistry.Municipality.Language.German:
                    item.NameGerman = value;
                    break;
                case StreetNameRegistry.Municipality.Language.English:
                    item.NameEnglish = value;
                    break;
                default: throw new InvalidOperationException($"Name language '{language}' has no mapping.");
            }
        }

        public static void UpdateHomonymAdditionByLanguage(this StreetNameVersion item, StreetNameRegistry.Municipality.Language language, string value)
        {
            switch (language)
            {
                case StreetNameRegistry.Municipality.Language.Dutch:
                    item.HomonymAdditionDutch = value;
                    break;
                case StreetNameRegistry.Municipality.Language.French:
                    item.HomonymAdditionFrench = value;
                    break;
                case StreetNameRegistry.Municipality.Language.German:
                    item.HomonymAdditionGerman = value;
                    break;
                case StreetNameRegistry.Municipality.Language.English:
                    item.HomonymAdditionEnglish = value;
                    break;
                default: throw new InvalidOperationException($"HomonymAddition language '{language}' has no mapping.");
            }
        }
    }
}
