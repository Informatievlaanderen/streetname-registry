namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class ApproveStreetName
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamAfgekeurdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
