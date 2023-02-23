namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class StreetNameWasProposedV2Extensions
    {
        public static StreetNameWasProposedV2 WithPersistentLocalId(this StreetNameWasProposedV2 @event,
            PersistentLocalId id)
        {
            var newEvent = new StreetNameWasProposedV2(
                new MunicipalityId(@event.MunicipalityId),
                new NisCode(@event.NisCode),
                new Names(@event.StreetNameNames),
                id);

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasProposedV2 WithNames(
            this StreetNameWasProposedV2 @event,
            Names names)
        {
            var newEvent = new StreetNameWasProposedV2(
                new MunicipalityId(@event.MunicipalityId),
                new NisCode(@event.NisCode),
                names,
                new PersistentLocalId(@event.PersistentLocalId));

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
