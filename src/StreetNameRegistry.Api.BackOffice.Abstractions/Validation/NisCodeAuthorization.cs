namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class NisCodeAuthorization
        {
            public static class NotAuthorized
            {
                public const string Message = "User has insufficient privileges to make edit changes on the municipality.";
            }
        }
    }
}
