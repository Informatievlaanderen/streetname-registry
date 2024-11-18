namespace StreetNameRegistry.Projections.Legacy.StreetNameDetailV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using NodaTime;

    [ConnectedProjectionName("API endpoint detail straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor het straatnamen detail voorziet.")]
    public sealed class StreetNameDetailProjectionsV2 : ConnectedProjection<LegacyContext>
    {
        public StreetNameDetailProjectionsV2()
        {
            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var streetNameDetailV2 = new StreetNameDetailV2
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = message.Message.IsRemoved,
                    Status = message.Message.Status
                };

                UpdateNameByLanguage(streetNameDetailV2, message.Message.Names);
                UpdateHomonymAdditionByLanguage(streetNameDetailV2, new HomonymAdditions(message.Message.HomonymAdditions));
                UpdateHash(streetNameDetailV2, message);

                await context
                    .StreetNameDetailV2
                    .AddAsync(streetNameDetailV2, ct);
            });
            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var streetNameDetailV2 = new StreetNameDetailV2
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = false,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameDetailV2, message.Message.StreetNameNames);
                UpdateHomonymAdditionByLanguage(streetNameDetailV2, new HomonymAdditions(message.Message.HomonymAdditions));
                UpdateHash(streetNameDetailV2, message);

                await context
                    .StreetNameDetailV2
                    .AddAsync(streetNameDetailV2, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var streetNameDetailV2 = new StreetNameDetailV2
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    PersistentLocalId = message.Message.PersistentLocalId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = false,
                    Status = StreetNameStatus.Proposed
                };

                UpdateNameByLanguage(streetNameDetailV2, message.Message.StreetNameNames);
                UpdateHash(streetNameDetailV2, message);

                await context
                    .StreetNameDetailV2
                    .AddAsync(streetNameDetailV2, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Current);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Proposed);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Rejected);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Rejected);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Proposed);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Retired);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Retired);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Retired);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Current);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateNameByLanguage(streetNameDetailV2, message.Message.StreetNameNames);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateNameByLanguage(streetNameDetailV2, message.Message.StreetNameNames);
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateHomonymAdditionByLanguage(streetNameDetailV2, new HomonymAdditions(message.Message.HomonymAdditions));
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case Language.Dutch: streetNameDetailV2.HomonymAdditionDutch = null;
                                break;
                            case Language.French: streetNameDetailV2.HomonymAdditionFrench = null;
                                break;
                            case Language.German: streetNameDetailV2.HomonymAdditionGerman = null;
                                break;
                            case Language.English: streetNameDetailV2.HomonymAdditionEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    streetNameDetailV2.Removed = true;
                    UpdateHash(streetNameDetailV2, message);
                    UpdateVersionTimestamp(streetNameDetailV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>((context, message, _) =>
            {
                var streetNamesToChange = context
                    .StreetNameDetailV2
                    .Local
                    .Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNameDetailV2.Where(s =>
                        s.MunicipalityId == message.Message.MunicipalityId));

                foreach (var streetNameDetailV2 in streetNamesToChange)
                    streetNameDetailV2.NisCode = message.Message.NisCode;

                return Task.CompletedTask;
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

        private static void UpdateHash<T>(StreetNameDetailV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateNameByLanguage(StreetNameDetailV2 entity, IDictionary<Language, string> streetNameNames)
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

        private static void UpdateHomonymAdditionByLanguage(StreetNameDetailV2 entity, List<StreetNameHomonymAddition> homonymAdditions)
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

        private static void UpdateVersionTimestamp(StreetNameDetailV2 streetName, Instant versionTimestamp)
            => streetName.VersionTimestamp = versionTimestamp;

        private static void UpdateStatus(StreetNameDetailV2 streetName, StreetNameStatus status)
            => streetName.Status = status;

        private static Task DoNothing<T>(LegacyContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
