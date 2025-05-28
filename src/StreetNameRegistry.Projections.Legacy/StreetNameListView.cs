namespace StreetNameRegistry.Projections.Legacy
{
    using System;
    using Municipality;

    public class StreetNameListView
    {
        public int StreetNamePersistentLocalId { get; set; }
        public string NisCode { get; set; }
        public string? StreetNameDutch { get; set; }
        public string? StreetNameFrench { get; set; }
        public string? StreetNameEnglish { get; set; }
        public string? StreetNameGerman { get; set; }

        public string? StreetNameDutchSearch { get; set; }
        public string? StreetNameFrenchSearch { get; set; }
        public string? StreetNameEnglishSearch { get; set; }
        public string? StreetNameGermanSearch { get; set; }

        public string? StreetNameHomonymAdditionDutch { get; set; }
        public string? StreetNameHomonymAdditionFrench { get; set; }
        public string? StreetNameHomonymAdditionGerman { get; set; }
        public string? StreetNameHomonymAdditionEnglish { get; set; }

        public StreetNameStatus? StreetNameStatus { get; set; }

        public string? MunicipalityNameDutchSearch { get; set; }
        public string? MunicipalityNameFrenchSearch { get; set; }
        public string? MunicipalityNameEnglishSearch { get; set; }
        public string? MunicipalityNameGermanSearch { get; set; }

        public Language? PrimaryLanguage { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }
        public bool IsInFlemishRegion { get; set; }
    }
}
