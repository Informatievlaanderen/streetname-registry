using System.Threading.Tasks;

namespace StreetNameRegistry.Api.BackOffice.Abstractions.Extensions
{
    using Municipality;
    using Municipality.Exceptions;

    public static class BackOfficeContextExtensions
    {
        public static async Task<MunicipalityId> GetMunicipalityIdByPersistentLocalId(
            this BackOfficeContext backOfficeContext,
            int streetNamePersistentLocalId)
        {
            var municipalityIdByPersistentLocalId = await backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .FindAsync(streetNamePersistentLocalId);

            if (municipalityIdByPersistentLocalId is null)
            {
                throw new StreetNameIsNotFoundException();
            }

            return new MunicipalityId(municipalityIdByPersistentLocalId.MunicipalityId);
        }
    }
}
