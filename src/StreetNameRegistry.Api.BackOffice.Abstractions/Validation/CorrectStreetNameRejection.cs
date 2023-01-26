namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameRejection
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'afgekeurd'.";
            }
        }
    }
}
