namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using MediatR;
    using Newtonsoft.Json;

    public class StreetNameCorrectRetirementRequest : StreetNameBackOfficeCorrectRetirementRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }
    }
}