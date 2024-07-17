namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.AspNetCore.Mvc.Infrastructure;

    public abstract class BackOfficeApiController : ApiController
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IProvenanceFactory _provenanceFactory;

        protected BackOfficeApiController(
            IActionContextAccessor actionContextAccessor,
            IProvenanceFactory provenanceFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _provenanceFactory = provenanceFactory;
        }

        protected IDictionary<string, object> GetMetadata()
        {
            var correlationId = _actionContextAccessor
                .ActionContext?
                .HttpContext
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
