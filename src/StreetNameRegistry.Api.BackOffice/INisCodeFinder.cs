namespace StreetNameRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface INisCodeFinder
    {
        Task<string?> FindNisCode<TRequest>(TRequest request, CancellationToken cancellationToken);
    }
}
