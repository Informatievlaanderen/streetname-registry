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

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue ConvertOsloFromMunicipalityStreetNameStatus(this Municipality.StreetNameStatus? status)
            => ConvertOsloFromMunicipalityStreetNameStatus(status ?? Municipality.StreetNameStatus.Current);

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue ConvertOsloFromMunicipalityStreetNameStatus(this Municipality.StreetNameStatus status)
        {
            switch (status)
            {
                case Municipality.StreetNameStatus.Retired:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue.Gehistoreerd;

                case Municipality.StreetNameStatus.Proposed:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue.Voorgesteld;

                case Municipality.StreetNameStatus.Rejected:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue.Afgekeurd;

                default:
                case Municipality.StreetNameStatus.Current:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam.StraatnaamStatusValue.InGebruik;
            }
        }
    }
}
