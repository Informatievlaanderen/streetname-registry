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

                // A predecessor is the version that ended that day, a successor the version that was added that day.
                StreetNameVersionLink Predecessor(int id) => new StreetNameVersionLink(id,
                    changes.TryGetValue(id, out var c) && c.Previous is not null ? c.Previous.State.VersionId : CurrentVersion(id));
                StreetNameVersionLink Successor(int id) => new StreetNameVersionLink(id,
                    changes.TryGetValue(id, out var c) && c.Current is not null ? c.Current.State.VersionId : CurrentVersion(id));

                // The versionIdentifier of a link is required by the XSD.
                DateTimeOffset CurrentVersion(int id) => versions.TryGetValue(id, out var version)
                    ? version
                    : throw new InvalidOperationException($"No version found for linked street name {id} on {day}.");

                var adds = changes
                    .Where(x => x.Value.Current is not null)
                    .Select(x => StreetNameMutationXml.BuildAdd(
                        x.Value.Current!,
                        (x.Value.Previous is null ? Enumerable.Empty<int>() : new[] { x.Key })
                            .Concat(x.Value.Current!.LinkedPersistentLocalIds)
                            .Select(Predecessor)));

                var updates = changes
                    .Where(x => x.Value.Previous is not null)
                    .Select(x => StreetNameMutationXml.BuildUpdate(
                        x.Value.Previous!,
                        (x.Value.Current is null ? Enumerable.Empty<int>() : new[] { x.Key })
                            .Concat(x.Value.Previous!.LinkedPersistentLocalIds)
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
        /// Merged street names can link to street names that did not change that day, their current version is used.
        /// </summary>
        private static async Task<IReadOnlyDictionary<int, DateTimeOffset>> GetVersionsOfStreetNamesWithoutChange(
            BosaContext context,
            IReadOnlyDictionary<int, StreetNameDayChange> changes,
            CancellationToken cancellationToken)
        {
            var ids = changes.Values
                .SelectMany(x => (x.Previous?.LinkedPersistentLocalIds ?? new List<int>()).Concat(x.Current?.LinkedPersistentLocalIds ?? new List<int>()))
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
