namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameApproval
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamGehistoreerdAfgekeurd";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
