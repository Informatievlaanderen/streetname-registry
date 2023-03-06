namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class StreetNameWasRejectedExtensions
    {
        public static StreetNameWasRejected WithPersistentLocalId(this StreetNameWasRejected @event, PersistentLocalId id)
        {
            var newEvent = new StreetNameWasRejected(
                new MunicipalityId(@event.MunicipalityId),
                id);

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
