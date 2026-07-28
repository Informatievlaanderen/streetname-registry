namespace StreetNameRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality.Events;
    using StreetName.Events;
    using StreetName.Events.Crab;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel straatnamen de gecachte data nog geüpdated moeten worden.")]
    public sealed class LastChangedProjectionsV3 : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering straatnamen (v3)";
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.JsonLd };

        public LastChangedProjectionsV3(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            When<Envelope<StreetNameWasMigrated>>(async (context, message, ct) =>
            {
                var attachedRecords =
                    await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);

                context.LastChangedList.RemoveRange(attachedRecords);
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

             When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.PersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                foreach (var streetNamePersistentLocalId in message.Message.StreetNamePersistentLocalIds)
                    await GetLastChangedRecordsAndUpdatePosition(streetNamePersistentLocalId.ToString(), message.Position, context, ct);
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
            When<Envelope<MunicipalityWasRemoved>>(DoNothing);
        }

        private static string GetIdentifier(string streetNamePersistentLocalId)
            => $"v3.{streetNamePersistentLocalId}";

        private static void RebuildKeyAndUri(IEnumerable<LastChangedRecord>? attachedRecords, int persistentLocalId)
        {
            if (attachedRecords == null)
            {
                return;
            }

            foreach (var record in attachedRecords)
            {
                if (record.CacheKey != null)
                {
                    record.CacheKey = string.Format(record.CacheKey, persistentLocalId);
                }

                if (record.Uri != null)
                {
                    record.Uri = string.Format(record.Uri, persistentLocalId);
                }
            }
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.JsonLd => $"oslo-v3/streetname:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => "/v3/straatnamen/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static Task DoNothing<T>(LastChangedListContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
