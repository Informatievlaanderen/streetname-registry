namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public static class StreetNameMaxLengthValidator
    {
        public const string Code = "StraatnaamMaxlengteValidatie";

        public static bool IsValid(KeyValuePair<Taal, string> streetName) => string.IsNullOrEmpty(streetName.Value) || streetName.Value.Length <= 60;
    }
}
