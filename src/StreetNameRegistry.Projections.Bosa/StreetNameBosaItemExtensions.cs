namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Municipality;

    public static class StreetNameBosaItemExtensions
    {
        public static async Task<StreetNameBosaItem> FindAndUpdateStreetName(
            this BosaContext context,
            int persistentLocalId,
            Action<StreetNameBosaItem> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNames
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);
            return streetName;
        }

        public static void UpdateNameByLanguage(this StreetNameBosaItem entity, IDictionary<Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.NameDutch = streetNameName;
                        break;

                    case Language.French:
                        entity.NameFrench = streetNameName;
                        break;

                    case Language.German:
                        entity.NameGerman = streetNameName;
                        break;

                    case Language.English:
                        entity.NameEnglish = streetNameName;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), streetNameName, null);
                }
            }
        }

        public static void UpdateHomonymAdditionByLanguage(this StreetNameBosaItem entity, IDictionary<Language, string> homonymAdditions)
        {
            foreach (var (language, homonymAddition) in homonymAdditions)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.HomonymAdditionDutch = homonymAddition;
                        break;

                    case Language.French:
                        entity.HomonymAdditionFrench = homonymAddition;
                        break;

                    case Language.German:
                        entity.HomonymAdditionGerman = homonymAddition;
                        break;

                    case Language.English:
                        entity.HomonymAdditionEnglish = homonymAddition;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }
            }
        }

        public static void RemoveHomonymAdditions(this StreetNameBosaItem entity, IEnumerable<Language> languages)
        {
            foreach (var language in languages)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.HomonymAdditionDutch = null;
                        break;

                    case Language.French:
                        entity.HomonymAdditionFrench = null;
                        break;

                    case Language.German:
                        entity.HomonymAdditionGerman = null;
                        break;

                    case Language.English:
                        entity.HomonymAdditionEnglish = null;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }
            }
        }

        private static ProjectionItemNotFoundException<StreetNameBosaProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameBosaProjections>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
