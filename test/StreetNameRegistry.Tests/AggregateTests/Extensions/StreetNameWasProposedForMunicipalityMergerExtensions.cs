namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using System.Linq;
    using Municipality;
    using Municipality.Events;

    public static class StreetNameWasProposedForMunicipalityMergerExtensions
    {
        public static StreetNameWasProposedForMunicipalityMerger WithPersistentLocalId(this StreetNameWasProposedForMunicipalityMerger @event,
            PersistentLocalId id)
        {
            var newEvent = new StreetNameWasProposedForMunicipalityMerger(
                new MunicipalityId(@event.MunicipalityId),
                new NisCode(@event.NisCode),
                @event.DesiredStatus,
                new Names(@event.StreetNameNames),
                new HomonymAdditions(@event.HomonymAdditions),
                id,
                @event.MergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)).ToList());

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasProposedForMunicipalityMerger WithMunicipalityId(this StreetNameWasProposedForMunicipalityMerger @event,
            MunicipalityId id)
        {
            var newEvent = new StreetNameWasProposedForMunicipalityMerger(
                id,
                new NisCode(@event.NisCode),
                @event.DesiredStatus,
                new Names(@event.StreetNameNames),
                new HomonymAdditions(@event.HomonymAdditions),
                new PersistentLocalId(@event.PersistentLocalId),
                @event.MergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)).ToList());

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasProposedForMunicipalityMerger WithMergedPersistentLocalIds(this StreetNameWasProposedForMunicipalityMerger @event,
            params PersistentLocalId[] mergedPersistentLocalIds)
        {
            var newEvent = new StreetNameWasProposedForMunicipalityMerger(
                new MunicipalityId(@event.MunicipalityId),
                new NisCode(@event.NisCode),
                @event.DesiredStatus,
                new Names(@event.StreetNameNames),
                new HomonymAdditions(@event.HomonymAdditions),
                new PersistentLocalId(@event.PersistentLocalId),
                mergedPersistentLocalIds.ToList());

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
