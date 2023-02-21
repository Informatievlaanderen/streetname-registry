namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameHomonymAdditions
        {
            public static class CannotAddHomonymAddition
            {
                public const string Code = "";
                public static string Message(string taalCode) => $"Er kan geen homoniemToevoeging worden toegevoegd voor taalcode {taalCode}.";
            }
        }
    }
}
