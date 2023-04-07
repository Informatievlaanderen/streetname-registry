namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    public sealed class ApproveStreetNameRequest : IHavePersistentLocalId
    {
        public int PersistentLocalId { get; set; }
    }
}
