namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using NodaTime;

    public enum StreetNameChangeType
    {
        /// <summary>
        /// Ends the current version and starts a new one, with the current version as predecessor.
        /// Every change of the state is a new version, corrections included.
        /// </summary>
        NewVersion,

        /// <summary>
        /// Ends the current version without a new one (removal, or replaced by other street names through a merger).
        /// The ended version keeps its state from before the change.
        /// </summary>
        End
    }

    public static class BosaContextExtensions
    {
        public static async Task CreateStreetName(
            this BosaContext context,
            BosaDay bosaDay,
            StreetNameBosaItem streetName,
            Instant timestamp,
            string eventType,
            long position,
            IEnumerable<int>? predecessors,
            CancellationToken ct)
        {
            streetName.VersionTimestamp = timestamp;
            streetName.CreatedOn = timestamp;
            streetName.VersionId = timestamp;
            streetName.BeginLifeSpan = timestamp;
            streetName.ValidFrom = timestamp;

            await context.StreetNames.AddAsync(streetName, ct);

            if (!bosaDay.TryGetDay(timestamp, out var day))
                return;

            var change = await GetOrAddChange(context, day, streetName.PersistentLocalId, ct);
            var data = change.GetData();

            data.Current = new StreetNameVersionPart
            {
                State = StreetNameVersionState.From(streetName),
                LinkedPersistentLocalIds = predecessors?.ToList() ?? new List<int>(),
                Events = { Event(timestamp, eventType) }
            };

            change.SetData(data);
            change.Position = position;
        }

        public static async Task<StreetNameBosaItem> ChangeStreetName(
            this BosaContext context,
            BosaDay bosaDay,
            int persistentLocalId,
            StreetNameChangeType type,
            Action<StreetNameBosaItem> update,
            Instant timestamp,
            string eventType,
            long position,
            CancellationToken ct,
            IEnumerable<int>? successors = null)
        {
            var streetName = await context.FindAndUpdateStreetName(persistentLocalId, _ => { }, ct);

            if (!bosaDay.TryGetDay(timestamp, out var day))
            {
                // Before Bosa receives changes, every change is a new version, like in the full download,
                // so the versions match what Bosa has when the changes start.
                update(streetName);
                streetName.VersionTimestamp = timestamp;
                streetName.VersionId = timestamp;
                streetName.ValidFrom = timestamp;
                return streetName;
            }

            var change = await GetOrAddChange(context, day, persistentLocalId, ct);
            var data = change.GetData();

            // The first change of the day of an existing street name: keep the version it started the day with.
            if (data.Previous is null && data.Current is null)
                data.Previous = new StreetNameVersionPart { State = StreetNameVersionState.From(streetName) };

            update(streetName);
            streetName.VersionTimestamp = timestamp;

            var @event = Event(timestamp, eventType);
            switch (type)
            {
                case StreetNameChangeType.NewVersion:
                    streetName.VersionId = timestamp;
                    if (data.Current is null)
                    {
                        // All new versions of a day are collapsed into one, which starts when the previous version ends.
                        streetName.BeginLifeSpan = timestamp;
                        streetName.ValidFrom = timestamp;

                        data.Previous!.EndLifeSpan ??= timestamp.ToDateTimeOffset();
                        data.Previous.Events.Add(@event);
                        data.Current = new StreetNameVersionPart();
                    }

                    data.Current.State = StreetNameVersionState.From(streetName);
                    data.Current.Events.Add(@event);
                    break;

                case StreetNameChangeType.End:
                    var ended = data.Current ?? data.Previous!;
                    ended.EndLifeSpan ??= timestamp.ToDateTimeOffset();
                    ended.LinkedPersistentLocalIds.AddRange(successors ?? Enumerable.Empty<int>());
                    ended.Events.Add(@event);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            change.SetData(data);
            change.Position = position;

            return streetName;
        }

        private static async Task<BosaStreetNameChange> GetOrAddChange(BosaContext context, DateOnly day, int persistentLocalId, CancellationToken ct)
        {
            var change = await context.StreetNameChanges.FindAsync(new object[] { day, persistentLocalId }, ct);
            if (change is not null)
                return change;

            change = new BosaStreetNameChange
            {
                Day = day,
                PersistentLocalId = persistentLocalId
            };

            await context.StreetNameChanges.AddAsync(change, ct);
            return change;
        }

        private static StreetNameVersionEvent Event(Instant timestamp, string type)
            => new StreetNameVersionEvent { Date = timestamp.ToDateTimeOffset(), Type = type };
    }
}
