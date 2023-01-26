namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class RetireStreetName
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamVoorgesteldOfAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
