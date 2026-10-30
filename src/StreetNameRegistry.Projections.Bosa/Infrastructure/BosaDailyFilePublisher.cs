namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NodaTime;

    public interface IBosaDailyFilePublisher
    {
        Task PublishAsync(BosaContext context, IEnumerable<DateOnly> days, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Rebuilds the add and update files of a day from the database and overwrites them in the file store.
    /// </summary>
    public sealed class BosaDailyFilePublisher : IBosaDailyFilePublisher
    {
        private readonly IBosaXmlWriter _xmlWriter;
        private readonly IBosaFileStore _fileStore;
        private readonly BosaOptions _options;

        public BosaDailyFilePublisher(
            IBosaXmlWriter xmlWriter,
            IBosaFileStore fileStore,
            IOptions<BosaOptions> options)
        {
            _xmlWriter = xmlWriter;
            _fileStore = fileStore;
            _options = options.Value;
        }

        public async Task PublishAsync(BosaContext context, IEnumerable<DateOnly> days, CancellationToken cancellationToken)
        {
            foreach (var day in days)
            {
                var changes = (await context.StreetNameChanges
                        .AsNoTracking()
                        .Where(x => x.Day == day)
                        .OrderBy(x => x.PersistentLocalId)
                        .ToListAsync(cancellationToken))
                    .ToDictionary(x => x.PersistentLocalId, x => x.GetData());

                var versions = await GetVersionsOfStreetNamesWithoutChange(context, changes, cancellationToken);

                // A successor is the version that was added that day, or the current version when there is none.
                StreetNameVersionLink Successor(int id) => new StreetNameVersionLink(id,
                    changes.TryGetValue(id, out var c) && c.Current is not null ? c.Current.State.VersionId : CurrentVersion(id));

                // The versionIdentifier of a link is required by the XSD.
                DateTimeOffset CurrentVersion(int id) => versions.TryGetValue(id, out var version)
                    ? version
                    : throw new InvalidOperationException($"No version found for linked street name {id} on {day}.");

                // The predecessors of a new version: its own previous version, and the versions that ended that day
                // with it as successor (merger, rename), so a street name proposed for a merger gets them when the merger happens.
                IEnumerable<StreetNameVersionLink> Predecessors(int id, StreetNameDayChange change)
                    => (change.Previous is null ? Enumerable.Empty<StreetNameVersionLink>() : new[] { new StreetNameVersionLink(id, change.Previous.State.VersionId) })
                        .Concat(changes
                            .Where(x => x.Key != id)
                            .SelectMany(x => new[] { x.Value.Previous, x.Value.Current })
                            .Where(x => x is not null && x.Successors.Contains(id))
                            .Select(x => new StreetNameVersionLink(x!.State.PersistentLocalId, x.State.VersionId)));

                var adds = changes
                    .Where(x => x.Value.Current is not null)
                    .Select(x => StreetNameMutationXml.BuildAdd(x.Value.Current!, Predecessors(x.Key, x.Value)));

                var updates = changes
                    .Where(x => x.Value.Previous is not null)
                    .Select(x => StreetNameMutationXml.BuildUpdate(
                        x.Value.Previous!,
                        (x.Value.Current is null ? Enumerable.Empty<int>() : new[] { x.Key })
                            .Concat(x.Value.Previous!.Successors)
                            .Select(Successor)));

                // Both files are always written, so the daily service finds a complete set for every day with changes.
                await Save(day, BosaChangeKind.Add, adds, cancellationToken);
                await Save(day, BosaChangeKind.Update, updates, cancellationToken);
            }
        }

        private async Task Save(DateOnly day, BosaChangeKind kind, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            _xmlWriter.Write(stream, kind, elements);
            stream.Position = 0;

            await _fileStore.SaveAsync(GetFileName(day, kind), stream, cancellationToken);
        }

        /// <summary>
        /// Ended street names can have successors that did not change that day, their current version is used.
        /// </summary>
        private static async Task<IReadOnlyDictionary<int, DateTimeOffset>> GetVersionsOfStreetNamesWithoutChange(
            BosaContext context,
            IReadOnlyDictionary<int, StreetNameDayChange> changes,
            CancellationToken cancellationToken)
        {
            var ids = changes.Values
                .SelectMany(x => (x.Previous?.Successors ?? new List<int>()).Concat(x.Current?.Successors ?? new List<int>()))
                .Where(x => !changes.ContainsKey(x))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<int, DateTimeOffset>();

            var streetNames = await context.StreetNames
                .AsNoTracking()
                .Where(x => ids.Contains(x.PersistentLocalId))
                .ToListAsync(cancellationToken);

            return streetNames.ToDictionary(x => x.PersistentLocalId, x => x.VersionId.ToDateTimeOffset());
        }

        public string GetFileName(DateOnly day, BosaChangeKind kind)
            => string.Format(CultureInfo.InvariantCulture, _options.FileNameFormat, day, kind == BosaChangeKind.Add ? "add" : "update");
    }

    public enum BosaChangeKind
    {
        Add = 1,
        Update = 2
    }
}
