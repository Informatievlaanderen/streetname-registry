namespace StreetNameRegistry.Api.BackOffice.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Municipality;
    using Municipality.Exceptions;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValid(string? ifMatchHeaderValue, PersistentLocalId streetNamePersistentLocalId, CancellationToken ct);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IMunicipalities _municipalities;

        public IfMatchHeaderValidator(BackOfficeContext backOfficeContext, IMunicipalities municipalities)
        {
            _backOfficeContext = backOfficeContext;
            _municipalities = municipalities;
        }

        public async Task<bool> IsValid(string? ifMatchHeaderValue, PersistentLocalId streetNamePersistentLocalId, CancellationToken ct)
        {
            if (ifMatchHeaderValue is null)
            {
                return true;
            }

            var municipalityIdByPersistentLocalId = await _backOfficeContext
                .MunicipalityIdByPersistentLocalId
                .FindAsync((int)streetNamePersistentLocalId);
            if (municipalityIdByPersistentLocalId is null)
            {
                throw new AggregateIdIsNotFoundException();
            }

            var municipalityId = new MunicipalityId(municipalityIdByPersistentLocalId.MunicipalityId);

            var ifMatchTag = ifMatchHeaderValue.Trim();

            var muniAggregate =
                await _municipalities.GetAsync(new MunicipalityStreamId(municipalityId), ct);
            var lastHash = muniAggregate.GetStreetNameHash(streetNamePersistentLocalId);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            return ifMatchTag == lastHashTag.ToString();
        }
    }
}
