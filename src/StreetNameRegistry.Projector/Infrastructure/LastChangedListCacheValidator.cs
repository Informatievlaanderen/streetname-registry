namespace StreetNameRegistry.Projector.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Projections.Legacy;

    public sealed class LastChangedListCacheValidator : ICacheValidator
    {
        private readonly LegacyContext _legacyContext;
        private readonly string _projectionName;

        public LastChangedListCacheValidator(LegacyContext legacyContext, string projectionName)
        {
            _legacyContext = legacyContext;
            _projectionName = projectionName;
        }

        public async Task<bool> CanCache(long position, CancellationToken ct)
        {
            var projectionPosition = await _legacyContext
                .ProjectionStates
                .AsNoTracking()
                .Where(ps => ps.Name == _projectionName)
                .Select(ps => ps.Position)
                .FirstOrDefaultAsync(ct);

            return projectionPosition >= position;
        }
    }
}
