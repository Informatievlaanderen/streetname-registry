namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameNames
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.";
            }
        }
    }
}
