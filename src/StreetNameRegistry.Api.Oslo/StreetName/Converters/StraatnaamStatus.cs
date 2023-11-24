namespace StreetNameRegistry.Api.Oslo.StreetName.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;

    public static class StraatnaamStatusExtensions
    {
        public static Municipality.StreetNameStatus ConvertToMunicipalityStreetNameStatus(this StraatnaamStatus status)
        {
            switch (status)
            {
                default:
                case StraatnaamStatus.InGebruik:
                    return Municipality.StreetNameStatus.Current;

                case StraatnaamStatus.Gehistoreerd:
                    return Municipality.StreetNameStatus.Retired;

                case StraatnaamStatus.Voorgesteld:
                    return Municipality.StreetNameStatus.Proposed;

                case StraatnaamStatus.Afgekeurd:
                    return Municipality.StreetNameStatus.Rejected;
            }
        }
    }
}
