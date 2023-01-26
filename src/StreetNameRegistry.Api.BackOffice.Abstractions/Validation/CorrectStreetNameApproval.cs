namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameApproval
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamGehistoreerdOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.";
            }
        }
    }
}
