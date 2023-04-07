namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    public sealed class CorrectStreetNameRetirementRequest : IHavePersistentLocalId
    {
        public int PersistentLocalId { get; set; }
    }
}
