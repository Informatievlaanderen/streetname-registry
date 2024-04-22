namespace StreetNameRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectStreetNameRetirement
        {
            public static class InvalidStatus
            {
                public const string Code = "StraatnaamAfgekeurdOfVoorgesteld";
                public const string Message = "Deze actie is enkel toegestaan op straatnamen met status 'gehistoreerd'.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }

            public static class RenamedStreetName
            {
                public const string Code = "StraatnaamHernoemd";
                public const string Message = "Deze actie is niet toegestaan op hernoemde straatnamen.";

                public static TicketError ToTicketError() => new TicketError(Message, Code);
            }
        }
    }
}
