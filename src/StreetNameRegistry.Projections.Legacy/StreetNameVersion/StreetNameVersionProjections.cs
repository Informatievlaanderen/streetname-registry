namespace StreetNameRegistry.Projections.Legacy.StreetNameVersion
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using StreetName.Events;

    public class StreetNameVersionProjections : ConnectedProjection<LegacyContext>
    {
        public StreetNameVersionProjections()
        {
            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                var streetNameVersionItem = new StreetNameVersion
                {
                    StreetNameId = message.Message.StreetNameId,
                    NisCode = message.Message.NisCode,
                    Position = message.Position,
                    Complete = false,
                    Removed = false
                };

                streetNameVersionItem.ApplyProvenance(message.Message.Provenance);

                await context
                    .StreetNameVersions
                    .AddAsync(streetNameVersionItem, ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var entities = await context.AllVersions(message.Message.StreetNameId, ct);

                foreach (var entity in entities)
                    entity.PersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateNameByLanguage(version, message.Message.Language, message.Message.Name),
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateNameByLanguage(version, message.Message.Language, message.Message.Name),
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateNameByLanguage(version, message.Message.Language, string.Empty),
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateNameByLanguage(version, message.Message.Language, string.Empty),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateHomonymAdditionByLanguage(version, message.Message.Language, message.Message.HomonymAddition),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateHomonymAdditionByLanguage(version, message.Message.Language, message.Message.HomonymAddition),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateHomonymAdditionByLanguage(version, message.Message.Language, string.Empty),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => UpdateHomonymAdditionByLanguage(version, message.Message.Language, string.Empty),
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Complete = true,
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Complete = false,
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Removed = true,
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Current,
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Proposed,
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Retired,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Current,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Proposed,
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = StreetNameStatus.Retired,
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = null,
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewStreetNameVersionItem(
                    message.Message.StreetNameId,
                    message,
                    version => version.Status = null,
                    ct);
            });
        }

        private static void UpdateNameByLanguage(StreetNameVersion entity, Language? language, string name)
        {
            switch (language)
            {
                case Language.Dutch:
                    entity.NameDutch = name;
                    break;

                case Language.French:
                    entity.NameFrench = name;
                    break;

                case Language.German:
                    entity.NameGerman = name;
                    break;

                case Language.English:
                    entity.NameEnglish = name;
                    break;
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameVersion entity, Language? language, string homonymAddition)
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
            }
        }
    }
}
