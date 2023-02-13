namespace StreetNameRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Municipality;
    using StreetNameRegistry.Infrastructure.Repositories;

    public class NisCodeFinderByPersistentLocalId : INisCodeFinder
    {
        private readonly StreetNames _streetNames;

        public NisCodeFinderByPersistentLocalId(StreetNames streetNames)
        {
            _streetNames = streetNames;
        }

        public async Task<string?> FindNisCode<TRequest>(TRequest request, CancellationToken cancellationToken)
        {
            await Task.Yield();
            if (typeof(TRequest) == typeof(ApproveStreetNameRequest))
            {
                var approveStreetNameRequest = request as ApproveStreetNameRequest;
                if (approveStreetNameRequest is null)
                {
                    return null;
                }

                var streetNamePersistentLocalId = new PersistentLocalId(approveStreetNameRequest!.PersistentLocalId);
                var streetName = await _streetNames.GetAsync(streetNamePersistentLocalId, cancellationToken);
                return streetName.NisCode;
            }

            return null;
        }
    }
}
