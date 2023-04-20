namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NisCodeService.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Municipality;

    public class StreetNameRegistryNisCodeFinder : INisCodeFinder<PersistentLocalId>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public StreetNameRegistryNisCodeFinder(BackOfficeContext backOfficeContext)
        {
            _backOfficeContext = backOfficeContext;
        }

        public async Task<string?> FindAsync(PersistentLocalId streetNamePersistentLocalId,  CancellationToken ct)
        {
            var result = await _backOfficeContext.FindMunicipalityIdByPersistentLocalId(streetNamePersistentLocalId, ct);;

            return result?.NisCode;
        }
    }
}
