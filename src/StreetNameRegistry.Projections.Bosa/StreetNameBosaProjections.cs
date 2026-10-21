namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;

    [ConnectedProjectionName("Bosa straatnaam")]
    [ConnectedProjectionDescription("Projectie die de volledige straatnaam data en de dagelijkse mutaties voor Bosa bijhoudt.")]
    public class StreetNameBosaProjections : ConnectedProjection<BosaContext>
    {
        public const string StreetMergeEventType = "streetMerge";

        public StreetNameBosaProjections(IOptions<BosaOptions> options, BosaDay bosaDay)
        {
            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNamePersistentLocalIds = await context.StreetNames
                    .Where(x => x.MunicipalityId == message.Message.MunicipalityId)
                    .Select(x => x.PersistentLocalId)
                    .ToListAsync(ct);

                foreach (var persistentLocalId in streetNamePersistentLocalIds)
                    await Change(context, message, persistentLocalId, StreetNameChangeType.NewVersion, item => item.NisCode = message.Message.NisCode, ct);
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var item = new StreetNameBosaItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    Status = message.Message.Status,
                    IsRemoved = message.Message.IsRemoved,
                    Namespace = options.Value.Namespace
                };

                item.UpdateNameByLanguage(message.Message.Names);
                item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions);

                await Create(context, message, item, null, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = new StreetNameBosaItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    Status = StreetNameStatus.Proposed,
                    IsRemoved = false,
                    Namespace = options.Value.Namespace
                };

                item.UpdateNameByLanguage(message.Message.StreetNameNames);
                item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions);

                await Create(context, message, item, message.Message.MergedStreetNamePersistentLocalIds, ct, StreetMergeEventType);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var item = new StreetNameBosaItem
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    Status = StreetNameStatus.Proposed,
                    IsRemoved = false,
                    Namespace = options.Value.Namespace
                };

                item.UpdateNameByLanguage(message.Message.StreetNameNames);

                await Create(context, message, item, null, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Current, ct));

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Proposed, ct));

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Rejected, ct));

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.End, item => item.Status = StreetNameStatus.Rejected, ct,
                    message.Message.NewPersistentLocalIds, StreetMergeEventType));

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Proposed, ct));

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Retired, ct));

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.End, item => item.Status = StreetNameStatus.Retired, ct,
                    message.Message.NewPersistentLocalIds, StreetMergeEventType));

            // A renamed street name is replaced by the destination street name, like a merger.
            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.End, item => item.Status = StreetNameStatus.Retired, ct,
                    new[] { message.Message.DestinationPersistentLocalId }));

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.Status = StreetNameStatus.Current, ct));

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.UpdateNameByLanguage(message.Message.StreetNameNames), ct));

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.UpdateNameByLanguage(message.Message.StreetNameNames), ct));

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.UpdateHomonymAdditionByLanguage(message.Message.HomonymAdditions), ct));

            // Part of correcting the homonym additions.
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.NewVersion, item => item.RemoveHomonymAdditions(message.Message.Languages), ct));

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
                await Change(context, message, message.Message.PersistentLocalId, StreetNameChangeType.End, item => item.IsRemoved = true, ct));

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

            Task Create<T>(BosaContext context, Envelope<T> message, StreetNameBosaItem item, IEnumerable<int>? predecessors, CancellationToken ct, string? eventType = null)
                where T : IMessage, IHasProvenance
                => context.CreateStreetName(bosaDay, item, message.Message.Provenance.Timestamp, eventType ?? typeof(T).Name, message.Position, predecessors, ct);

            Task Change<T>(BosaContext context, Envelope<T> message, int persistentLocalId, StreetNameChangeType type, Action<StreetNameBosaItem> update, CancellationToken ct,
                IEnumerable<int>? successors = null, string? eventType = null)
                where T : IMessage, IHasProvenance
                => context.ChangeStreetName(bosaDay, persistentLocalId, type, update, message.Message.Provenance.Timestamp, eventType ?? typeof(T).Name, message.Position, ct, successors);
        }

        private static Task DoNothing<T>(BosaContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
