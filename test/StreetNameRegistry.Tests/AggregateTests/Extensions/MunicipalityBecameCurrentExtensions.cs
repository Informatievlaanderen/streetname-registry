namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class MunicipalityBecameCurrentExtensions
    {
        public static MunicipalityBecameCurrent WithMunicipalityId(this MunicipalityBecameCurrent @event, MunicipalityId municipalityId)
        {
            var newEvent = new MunicipalityBecameCurrent(municipalityId);

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
