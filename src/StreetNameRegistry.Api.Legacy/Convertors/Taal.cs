namespace StreetNameRegistry.Api.Legacy.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public static class TaalExtensions
    {
        public static Municipality.Language ConvertToMunicipalityLanguage(this Taal taal)
        {
            switch (taal)
            {
                default:
                case Taal.NL:
                    return Municipality.Language.Dutch;

                case Taal.FR:
                    return Municipality.Language.French;

                case Taal.DE:
                    return Municipality.Language.German;

                case Taal.EN:
                    return Municipality.Language.English;
            }
        }
    }
}
