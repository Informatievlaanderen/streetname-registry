namespace StreetNameRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality.Events;
    using StreetName.Events;
    using StreetName.Events.Crab;

    [ConnectedProjectionName("Cache markering straatnamen")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel straatnamen de gecachte data nog geüpdated moeten worden.")]
    public sealed class LastChangedProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

        public LastChangedProjections()
            : base(SupportedAcceptTypes)
        {
            #region Legacy Events
            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);

                RebuildKeyAndUri(attachedRecords, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, message, ct) => await DoNothing());

            When<Envelope<StreetNameWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<StreetNameStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());

            #endregion

           When<Envelope<StreetNameWasMigrated>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.StreetNameId.ToString(), message.Position, context, ct);

                context.LastChangedList.RemoveRange(attachedRecords);
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.PersistentLocalId);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.PersistentLocalId.ToString(), message.Position, context, ct);
            });
        }

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
                AcceptType.Json => $"legacy/streetname:{{0}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/streetname:{{0}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/streetname:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => "/v1/straatnamen/{0}",
                AcceptType.Xml => "/v1/straatnamen/{0}",
                AcceptType.JsonLd => "/v2/straatnamen/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
