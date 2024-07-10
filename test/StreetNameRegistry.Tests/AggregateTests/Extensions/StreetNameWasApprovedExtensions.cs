
namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class StreetNameWasApprovedExtensions
    {
        public static StreetNameWasApproved WithPersistentLocalId(this StreetNameWasApproved @event,
            PersistentLocalId id)
        {
            var newEvent = new StreetNameWasApproved(
                new MunicipalityId(@event.MunicipalityId),
                id);

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
