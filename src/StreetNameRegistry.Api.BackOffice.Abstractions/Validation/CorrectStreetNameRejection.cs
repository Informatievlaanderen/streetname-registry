namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameRejection
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamInGebruikOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'afgekeurd'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
