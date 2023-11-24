namespace StreetNameRegistry.Api.Oslo.StreetName.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;

    public static class StreetNameStatusExtensions
    {
        public static StraatnaamStatus ConvertFromMunicipalityStreetNameStatus(this Municipality.StreetNameStatus? status)
            => ConvertFromMunicipalityStreetNameStatus(status ?? Municipality.StreetNameStatus.Current);

        public static StraatnaamStatus ConvertFromMunicipalityStreetNameStatus(this Municipality.StreetNameStatus status)
        {
            switch (status)
            {
                case Municipality.StreetNameStatus.Retired:
                    return StraatnaamStatus.Gehistoreerd;

                case Municipality.StreetNameStatus.Proposed:
                    return StraatnaamStatus.Voorgesteld;

                case Municipality.StreetNameStatus.Rejected:
                    return StraatnaamStatus.Afgekeurd;

                default:
                case Municipality.StreetNameStatus.Current:
                    return StraatnaamStatus.InGebruik;
            }
        }
    }
}
