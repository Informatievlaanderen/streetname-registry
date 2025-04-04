namespace StreetNameRegistry.Projections.Legacy.StreetNameListV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using NodaTime;

    [ConnectedProjectionName("API endpoint lijst straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor de straatnamen lijst voorziet.")]
    public sealed class StreetNameListProjectionsV2 : ConnectedProjection<LegacyContext>
    {
        public StreetNameListProjectionsV2()
        {
            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId,
                        cancellationToken: ct);

                var streetNameListItemV2 = new StreetNameListItemV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    IsInFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = message.Message.IsRemoved,
                    PrimaryLanguage = municipality.PrimaryLanguage,
                    Status = message.Message.Status
                };

                UpdateNameByLanguage(streetNameListItemV2, message.Message.Names);
                UpdateHomonymAdditionByLanguage(streetNameListItemV2, new HomonymAdditions(message.Message.HomonymAdditions));

                await context
                    .StreetNameListV2
                    .AddAsync(streetNameListItemV2, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId,
                        cancellationToken: ct);

                var streetNameListItemV2 = new StreetNameListItemV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    IsInFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = false,
                    PrimaryLanguage = municipality.PrimaryLanguage,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameListItemV2, message.Message.StreetNameNames);
                UpdateHomonymAdditionByLanguage(streetNameListItemV2, new HomonymAdditions(message.Message.HomonymAdditions));

                await context
                    .StreetNameListV2
                    .AddAsync(streetNameListItemV2, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId,
                        cancellationToken: ct);

                var streetNameListItemV2 = new StreetNameListItemV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    IsInFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = false,
                    PrimaryLanguage = municipality.PrimaryLanguage,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameListItemV2, message.Message.StreetNameNames);

                await context
                    .StreetNameListV2
                    .AddAsync(streetNameListItemV2, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Current);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Proposed);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Rejected);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Rejected);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Proposed);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Retired);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Retired);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Retired);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateStatus(streetNameListItemV2, StreetNameStatus.Current);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateNameByLanguage(streetNameListItemV2, message.Message.StreetNameNames);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateNameByLanguage(streetNameListItemV2, message.Message.StreetNameNames);
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        UpdateHomonymAdditionByLanguage(streetNameListItemV2, new HomonymAdditions(message.Message.HomonymAdditions));
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        foreach (var language in message.Message.Languages)
                        {
                            switch (language)
                            {
                                case Language.Dutch: streetNameListItemV2.HomonymAdditionDutch = null;
                                    break;
                                case Language.French: streetNameListItemV2.HomonymAdditionFrench = null;
                                    break;
                                case Language.German: streetNameListItemV2.HomonymAdditionGerman = null;
                                    break;
                                case Language.English: streetNameListItemV2.HomonymAdditionEnglish = null;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<MunicipalityWasImported>>(async (context, message, ct) =>
            {
                var streetNameListMunicipality = new StreetNameListMunicipality
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode
                };

                await context
                    .StreetNameListMunicipality
                    .AddAsync(streetNameListMunicipality, ct);
            });

            When<Envelope<MunicipalityOfficialLanguageWasAdded>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId, cancellationToken: ct);

                if (municipality.PrimaryLanguage == null)
                {
                    municipality.PrimaryLanguage = message.Message.Language;
                }
                else if (municipality.SecondaryLanguage == null)
                {
                    municipality.SecondaryLanguage = message.Message.Language;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot add an official language while primary and secondary are assigned for municipality {municipality.MunicipalityId}");
                }
            });

            When<Envelope<MunicipalityOfficialLanguageWasRemoved>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId, cancellationToken: ct);

                if (municipality.SecondaryLanguage == message.Message.Language)
                {
                    municipality.SecondaryLanguage = null;
                }
                else if (municipality.PrimaryLanguage == message.Message.Language)
                {
                    municipality.PrimaryLanguage = null;

                    // if official is removed for primary, but still has secondary, then move secondary to primary
                    if (municipality.SecondaryLanguage != null)
                    {
                        municipality.PrimaryLanguage = municipality.SecondaryLanguage;
                    }
                }
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var municipality =
                    await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId,
                        cancellationToken: ct);

                municipality.NisCode = message.Message.NisCode;

                var streetNames = context
                    .StreetNameListV2
                    .Local
                    .Where(s => s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNameListV2.Where(s => s.MunicipalityId == message.Message.MunicipalityId));

                var streetNameIsInFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode);
                foreach (var streetName in streetNames)
                {
                    streetName.NisCode = message.Message.NisCode;
                    streetName.IsInFlemishRegion = streetNameIsInFlemishRegion;
                }
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameListItem(
                    message.Message.PersistentLocalId, streetNameListItemV2 =>
                    {
                        streetNameListItemV2.Removed = true;
                        UpdateVersionTimestamp(streetNameListItemV2, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<MunicipalityBecameCurrent>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityOfficialLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityOfficialLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Envelope<MunicipalityWasImported>>(DoNothing);
            When<Envelope<MunicipalityWasMerged>>(DoNothing);
            When<Envelope<MunicipalityWasNamed>>(DoNothing);
            When<Envelope<MunicipalityWasRetired>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);
        }

        private static void UpdateNameByLanguage(StreetNameListItemV2 entity, IDictionary<Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.NameDutch = streetNameName;
                        entity.NameDutchSearch = streetNameName.RemoveDiacritics();
                        break;

                    case Language.French:
                        entity.NameFrench = streetNameName;
                        entity.NameFrenchSearch = streetNameName.RemoveDiacritics();
                        break;

                    case Language.German:
                        entity.NameGerman = streetNameName;
                        entity.NameGermanSearch = streetNameName.RemoveDiacritics();
                        break;

                    case Language.English:
                        entity.NameEnglish = streetNameName;
                        entity.NameEnglishSearch = streetNameName.RemoveDiacritics();
                        break;
                }
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameListItemV2 entity, List<StreetNameHomonymAddition> homonymAdditions)
        {
            foreach (var homonymAddition in homonymAdditions)
            {
                switch (homonymAddition.Language)
                {
                    case Language.Dutch:
                        entity.HomonymAdditionDutch = homonymAddition.HomonymAddition;
                        break;

                    case Language.French:
                        entity.HomonymAdditionFrench = homonymAddition.HomonymAddition;
                        break;

                    case Language.German:
                        entity.HomonymAdditionGerman = homonymAddition.HomonymAddition;
                        break;

                    case Language.English:
                        entity.HomonymAdditionEnglish = homonymAddition.HomonymAddition;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(homonymAddition.Language), homonymAddition.Language, null);
                }
            }
        }

        private static void UpdateVersionTimestamp(StreetNameListItemV2 streetNameListItemV2, Instant timestamp)
            => streetNameListItemV2.VersionTimestamp = timestamp;

        private static void UpdateStatus(StreetNameListItemV2 streetNameListItemV2, StreetNameStatus status)
            => streetNameListItemV2.Status = status;

        private static Task DoNothing<T>(LegacyContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
