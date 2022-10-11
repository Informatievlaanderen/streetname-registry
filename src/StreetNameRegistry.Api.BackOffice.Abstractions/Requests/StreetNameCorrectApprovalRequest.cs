namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using MediatR;
    using Newtonsoft.Json;
    using Response;

    public class StreetNameCorrectApprovalRequest : StreetNameBackOfficeCorrectApprovalRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }
    }
}
