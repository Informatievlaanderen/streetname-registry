namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Infrastructure;

    public class BosaContext : RunnerDbContext<BosaContext>
    {
        private readonly IBosaDailyFilePublisher? _publisher;

        public override string ProjectionStateSchema => Schema.Bosa;

        public DbSet<StreetNameBosaItem> StreetNames => Set<StreetNameBosaItem>();
        public DbSet<BosaStreetNameChange> StreetNameChanges => Set<BosaStreetNameChange>();

        // This needs to be here to please EF
        public BosaContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public BosaContext(DbContextOptions<BosaContext> options)
            : base(options) { }

        public BosaContext(DbContextOptions<BosaContext> options, IBosaDailyFilePublisher publisher)
            : base(options)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// The projection runner calls this once per batch of events.
        /// The changes and the projection position are saved in one transaction, and before committing,
        /// the file of every day touched by the batch is rebuilt from the database and uploaded.
        /// A failed upload rolls back the batch, so it is retried, and a failed commit after an upload
        /// leads to the same batch being projected and uploaded again, which is idempotent.
        /// </summary>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var days = GetChangedDays();
            if (_publisher is null || days.Count == 0)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            if (!Database.IsRelational())
            {
                var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                await _publisher.PublishAsync(this, days, cancellationToken);
                return saved;
            }

            var result = await Database.CreateExecutionStrategy().ExecuteAsync(async ct =>
            {
                await using var transaction = await Database.BeginTransactionAsync(ct);

                var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct);
                await _publisher.PublishAsync(this, days, ct);

                await transaction.CommitAsync(ct);
                return saved;
            }, cancellationToken);

            if (acceptAllChangesOnSuccess)
                ChangeTracker.AcceptAllChanges();

            return result;
        }

        private IReadOnlyCollection<DateOnly> GetChangedDays()
            => ChangeTracker
                .Entries<BosaStreetNameChange>()
                .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(x => x.Entity.Day)
                .Distinct()
                .ToList();
    }
}
