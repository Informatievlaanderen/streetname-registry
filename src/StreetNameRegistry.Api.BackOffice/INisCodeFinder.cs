namespace StreetNameRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;

    public interface INisCodeFinder
    {
        Task<string?> FindNisCode<TRequest>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IHavePersistentLocalId;
    }
}
