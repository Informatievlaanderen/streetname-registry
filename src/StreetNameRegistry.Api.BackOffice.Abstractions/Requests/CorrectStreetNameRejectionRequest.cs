namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    public sealed class CorrectStreetNameRejectionRequest : IHavePersistentLocalId
    {
        public int PersistentLocalId { get; set; }
    }
}
