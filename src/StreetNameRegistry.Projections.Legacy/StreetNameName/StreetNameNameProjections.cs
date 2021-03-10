namespace StreetNameRegistry.Projections.Legacy.StreetNameName
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using StreetName.Events;
    using StreetName.Events.Crab;

    [ConnectedProjectionName("Legacy - StreetNameName")]
    [ConnectedProjectionDescription("Straatnaam naam data (BOSA).")]
    public class StreetNameNameProjections : ConnectedProjection<LegacyContext>
    {
        public StreetNameNameProjections()
        {
            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .StreetNameNames
                    .AddAsync(new StreetNameName
                    {
                        StreetNameId = message.Message.StreetNameId,
                        NisCode = message.Message.NisCode,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        IsFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode),
                        Complete = false,
                        Removed = false
                    }, ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        UpdateNameByLanguage(streetNameNameItem, message.Message.Language, message.Message.Name);
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        UpdateNameByLanguage(streetNameNameItem, message.Message.Language, message.Message.Name);
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        UpdateNameByLanguage(streetNameNameItem, message.Message.Language, null);
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        UpdateNameByLanguage(streetNameNameItem, message.Message.Language, null);
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Complete = true;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Complete = false;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Removed = true;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Current;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Proposed;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Retired;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Current;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Proposed;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = StreetNameStatus.Retired;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = null;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.Status = null;
                        UpdateVersionTimestamp(streetNameNameItem, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(
                    message.Message.StreetNameId,
                    streetNameNameItem =>
                    {
                        streetNameNameItem.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, message, ct) => DoNothing());

            When<Envelope<StreetNameWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void UpdateNameByLanguage(StreetNameName streetNameName, Language? language, string name)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    streetNameName.NameDutch = name;
                    streetNameName.NameDutchSearch = name?.SanitizeForBosaSearch();
                    break;

                case Language.French:
                    streetNameName.NameFrench = name;
                    streetNameName.NameFrenchSearch = name?.SanitizeForBosaSearch();
                    break;

                case Language.German:
                    streetNameName.NameGerman = name;
                    streetNameName.NameGermanSearch = name?.SanitizeForBosaSearch();
                    break;

                case Language.English:
                    streetNameName.NameEnglish = name;
                    streetNameName.NameEnglishSearch = name?.SanitizeForBosaSearch();
                    break;
            }
        }

        private static void UpdateVersionTimestamp(StreetNameName streetNameName, Instant versionTimestamp)
            => streetNameName.VersionTimestamp = versionTimestamp;

        private static void DoNothing() { }
    }
}
