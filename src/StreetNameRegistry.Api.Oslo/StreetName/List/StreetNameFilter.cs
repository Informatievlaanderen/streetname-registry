namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    public class StreetNameFilter
    {
        public string StreetNameName { get; set; }
        public string MunicipalityName { get; set; }
        public string Status { get; set; }
        public string? NisCode { get; set; }
        public bool? IsInFlemishRegion { get; set; } = null;
    }
}
