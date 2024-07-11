namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Municipality;
    using Municipality.Events;

    public static class MunicipalityWasImportedExtensions
    {
        public static MunicipalityWasImported WithMunicipalityId(this MunicipalityWasImported @event, MunicipalityId municipalityId)
        {
            var newEvent = new MunicipalityWasImported(municipalityId, new NisCode(@event.NisCode));

            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
