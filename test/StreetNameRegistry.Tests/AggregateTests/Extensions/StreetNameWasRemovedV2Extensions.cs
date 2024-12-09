
namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class StreetNameWasRemovedV2Extensions
    {
        public static StreetNameWasRemovedV2 WithPersistentLocalId(this StreetNameWasRemovedV2 @event,
            PersistentLocalId id)
        {
            var newEvent = new StreetNameWasRemovedV2(
                new MunicipalityId(@event.MunicipalityId),
                id);

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasRemovedV2 WithMunicipalityId(this StreetNameWasRemovedV2 @event,
            MunicipalityId id)
        {
            var newEvent = new StreetNameWasRemovedV2(
                id,
                new PersistentLocalId(@event.PersistentLocalId));

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
