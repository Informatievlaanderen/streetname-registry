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
            where TRequest : IHavePersistentLocalId
        {
            var streetNamePersistentLocalId = new PersistentLocalId(request!.PersistentLocalId);
            var streetName = await _streetNames.GetAsync(streetNamePersistentLocalId, cancellationToken);
            return streetName.NisCode;

            // switch (request)
            // {
            //     case ApproveStreetNameRequest approveStreetNameRequest:
            //     {
            //         var streetNamePersistentLocalId = new PersistentLocalId(approveStreetNameRequest!.PersistentLocalId);
            //         var streetName = await _streetNames.GetAsync(streetNamePersistentLocalId, cancellationToken);
            //         return streetName.NisCode;
            //     }
            //     case CorrectStreetNameApprovalRequest correctStreetNameApprovalRequest:
            //     {
            //         var streetNamePersistentLocalId = new PersistentLocalId(correctStreetNameApprovalRequest!.PersistentLocalId);
            //         var streetName = await _streetNames.GetAsync(streetNamePersistentLocalId, cancellationToken);
            //         return streetName.NisCode;
            //     }
            //     case CorrectStreetNameRejectionRequest correctStreetNameRejectionRequest:
            //     {
            //         var streetNamePersistentLocalId = new PersistentLocalId(correctStreetNameRejectionRequest!.PersistentLocalId);
            //         var streetName = await _streetNames.GetAsync(streetNamePersistentLocalId, cancellationToken);
            //         return streetName.NisCode;
            //     }
            //
            //     // TODO: add other request types
            //
            //     default:
            //         // ...
            //
            //         return null;
            // }
        }
    }
}
