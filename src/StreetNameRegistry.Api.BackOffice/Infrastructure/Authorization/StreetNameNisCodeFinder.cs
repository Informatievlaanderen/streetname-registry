namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Municipality;

    public class StreetNameNisCodeFinder : INisCodeFinder<PersistentLocalId>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public StreetNameNisCodeFinder(BackOfficeContext backOfficeContext)
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
