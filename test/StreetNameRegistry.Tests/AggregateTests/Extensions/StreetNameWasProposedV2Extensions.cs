namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
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

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static StreetNameWasProposedV2 WithMunicipalityId(this StreetNameWasProposedV2 @event,
            MunicipalityId id)
        {
            var newEvent = new StreetNameWasProposedV2(
                id,
                new NisCode(@event.NisCode),
                new Names(@event.StreetNameNames),
                new PersistentLocalId(@event.PersistentLocalId));

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

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

            newEvent.SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
