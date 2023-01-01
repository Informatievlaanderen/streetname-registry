namespace StreetNameRegistry.Api.Oslo.Microsoft.StreetName.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;

    public static class StreetNameStatusExtensions
    {
        public static StraatnaamStatus ConvertFromStreetNameStatus(this StreetNameRegistry.StreetName.StreetNameStatus? status)
            => ConvertFromStreetNameStatus(status ?? StreetNameRegistry.StreetName.StreetNameStatus.Current);

        public static StraatnaamStatus ConvertFromStreetNameStatus(this StreetNameRegistry.StreetName.StreetNameStatus status)
        {
            switch (status)
            {
                case StreetNameRegistry.StreetName.StreetNameStatus.Retired:
                    return StraatnaamStatus.Gehistoreerd;

                case StreetNameRegistry.StreetName.StreetNameStatus.Proposed:
                    return StraatnaamStatus.Voorgesteld;

                default:
                case StreetNameRegistry.StreetName.StreetNameStatus.Current:
                    return StraatnaamStatus.InGebruik;
            }
        }

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
