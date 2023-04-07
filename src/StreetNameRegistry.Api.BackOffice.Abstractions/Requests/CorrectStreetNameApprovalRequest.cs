namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    public sealed class CorrectStreetNameApprovalRequest : IHavePersistentLocalId
    {
        public int PersistentLocalId { get; set; }
    }
}
