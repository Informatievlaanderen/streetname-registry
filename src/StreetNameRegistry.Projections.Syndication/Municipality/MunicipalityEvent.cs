namespace StreetNameRegistry.Projections.Syndication.Municipality
{
    public enum MunicipalityEvent
    {
        MunicipalityWasRegistered,
        MunicipalityWasNamed,
        MunicipalityNameWasCleared,
        MunicipalityNameWasCorrected,
        MunicipalityNameWasCorrectedToCleared,
        MunicipalityNisCodeWasDefined,
        MunicipalityNisCodeWasCorrected,
        MunicipalityOfficialLanguageWasAdded,
        MunicipalityOfficialLanguageWasRemoved,
        MunicipalityFacilitiesLanguageWasAdded,
        MunicipalityFacilitiesLanguageWasRemoved,

        MunicipalityBecameCurrent,
        MunicipalityWasCorrectedToCurrent,
        MunicipalityWasRetired,
        MunicipalityWasCorrectedToRetired,

        MunicipalityWasMerged
    }
}
