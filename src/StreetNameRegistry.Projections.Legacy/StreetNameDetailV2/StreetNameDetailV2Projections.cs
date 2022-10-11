namespace StreetNameRegistry.Projections.Legacy.StreetNameDetailV2
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using NodaTime;
    using StreetName.Events;
    using StreetNameName = Municipality.StreetNameName;

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

                UpdateNameByLanguage(streetNameDetailV2, new Names(message.Message.Names));
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

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameDetailV2(message.Message.PersistentLocalId, streetNameDetailV2 =>
                {
                    UpdateStatus(streetNameDetailV2, StreetNameStatus.Retired);
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
        }

        private static void UpdateHash<T>(StreetNameDetailV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateNameByLanguage(StreetNameDetailV2 entity, List<StreetNameName> streetNameNames)
        {
            foreach (var streetNameName in streetNameNames)
            {
                switch (streetNameName.Language)
                {
                    case Language.Dutch:
                        entity.NameDutch = streetNameName.Name;
                        break;

                    case Language.French:
                        entity.NameFrench = streetNameName.Name;
                        break;

                    case Language.German:
                        entity.NameGerman = streetNameName.Name;
                        break;

                    case Language.English:
                        entity.NameEnglish = streetNameName.Name;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(streetNameName.Language), streetNameName.Language, null);
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
    }
}
