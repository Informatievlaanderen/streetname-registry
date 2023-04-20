namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Convertors;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using NisCodeService.Abstractions;

    public class MunicipalityRegistryNisCodeFinder : INisCodeFinder<MunicipalityPuri>
    {
        public async Task<string?> FindAsync(MunicipalityPuri municipalityPuri,  CancellationToken ct)
        {
            try
            {
                var identifier = municipalityPuri.Puri
                    .AsIdentifier()
                    .Map(IdentifierMappings.MunicipalityNisCode);

                return identifier.Value;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }
    }

    public record MunicipalityPuri(string Puri);
}
