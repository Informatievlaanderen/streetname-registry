namespace StreetNameRegistry.Projections.Legacy.StreetNameList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using StreetName.Events;
    using StreetName.Events.Crab;

    [ConnectedProjectionName("Legacy - StreetNameList")]
    [ConnectedProjectionDescription("Straatnaam lijst data.")]
    public class StreetNameListProjections : ConnectedProjection<LegacyContext>
    {
        public StreetNameListProjections()
        {
            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .StreetNameList
                    .AddAsync(
                        new StreetNameListItem
                        {
                            StreetNameId = message.Message.StreetNameId,
                            NisCode = message.Message.NisCode,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            Complete = false,
                            Removed = false
                        }, ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateNameByLanguage(entity, message.Message.Language, message.Message.Name);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateNameByLanguage(entity, message.Message.Language, message.Message.Name);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateNameByLanguage(entity, message.Message.Language, string.Empty);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateNameByLanguage(entity, message.Message.Language, string.Empty);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateHomonymAdditionByLanguage(entity, message.Message.Language, message.Message.HomonymAddition);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateHomonymAdditionByLanguage(entity, message.Message.Language, message.Message.HomonymAddition);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateHomonymAdditionByLanguage(entity, message.Message.Language, string.Empty);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        UpdateHomonymAdditionByLanguage(entity, message.Message.Language, string.Empty);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Complete = true;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Complete = false;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Removed = true;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity => entity.PrimaryLanguage = message.Message.PrimaryLanguage,
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity => entity.PrimaryLanguage = null,
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity => entity.PrimaryLanguage = message.Message.PrimaryLanguage,
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity => entity.PrimaryLanguage = null,
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Current;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Current;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Proposed;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Proposed;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Retired;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = StreetNameStatus.Retired;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = null;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) => {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.StreetNameId,
                    entity =>
                    {
                        entity.Status = null;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, message, ct) => DoNothing());

            When<Envelope<StreetNameWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void UpdateNameByLanguage(StreetNameListItem entity, Language? language, string name)
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

        private static void UpdateHomonymAdditionByLanguage(StreetNameListItem entity, Language? language, string homonymAddition)
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

        private static void UpdateVersionTimestamp(StreetNameListItem streetNameListItem, Instant timestamp)
            => streetNameListItem.VersionTimestamp = timestamp;

        private static void DoNothing() { }
    }
}
