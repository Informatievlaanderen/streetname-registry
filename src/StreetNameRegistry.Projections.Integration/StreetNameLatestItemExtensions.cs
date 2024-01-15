namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Municipality;

    public static class StreetNameLatestItemExtensions
    {
        public static async Task FindAndUpdateStreetNameLatestItem(
            this IntegrationContext context,
            int persistentLocalId,
            Action<StreetNameLatestItem> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNameLatestItems
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName == null)
                throw DatabaseItemNotFound(persistentLocalId);

            updateFunc(streetName);
        }

        public static void UpdateNameByLanguage(this StreetNameLatestItem entity, IDictionary<Language, string> streetNameNames)
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

        public static void UpdateHomonymAdditionByLanguage(this StreetNameLatestItem entity, IDictionary<Language, string> homonymAdditions)
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

        private static ProjectionItemNotFoundException<StreetNameLatestItemProjections> DatabaseItemNotFound(int persistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameLatestItemProjections>(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
