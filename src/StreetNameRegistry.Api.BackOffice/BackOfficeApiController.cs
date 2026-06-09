namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.AspNetCore.Http;

    public abstract class BackOfficeApiController : ApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProvenanceFactory _provenanceFactory;

        protected BackOfficeApiController(
            IHttpContextAccessor httpContextAccessor,
            IProvenanceFactory provenanceFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _provenanceFactory = provenanceFactory;
        }

        protected IDictionary<string, object> GetMetadata()
        {
            var correlationId = _httpContextAccessor
                .HttpContext!
                .Request
                .Headers["x-correlation-id"].FirstOrDefault() ?? Guid.NewGuid().ToString("D");

            return new Dictionary<string, object>
            {
                { "CorrelationId", correlationId }
            };
        }

        protected Provenance CreateProvenance(Modification modification, string reason = "")
            => _provenanceFactory.Create(new Reason(reason), modification);
    }
}
