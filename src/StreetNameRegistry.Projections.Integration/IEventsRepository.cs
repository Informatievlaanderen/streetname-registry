namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;

    public interface IEventsRepository
    {
        Task<int?> GetPersistentLocalId(Guid addressId);
    }
}
