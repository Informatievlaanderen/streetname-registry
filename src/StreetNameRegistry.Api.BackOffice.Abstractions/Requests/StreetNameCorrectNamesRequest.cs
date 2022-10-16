namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Convertors;
    using MediatR;
    using Municipality;
    using Municipality.Commands;
    using Newtonsoft.Json;

    [DataContract(Name = "CorrigerenStraatnaamNamen", Namespace = "")]
    public class StreetNameCorrectNamesRequest : StreetNameBackOfficeCorrectNamesRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to CorrectStreetNameNames command
        /// </summary>
        /// <returns>CorrectStreetNameNames.</returns>
        public CorrectStreetNameNames ToCommand(MunicipalityId municipalityId, Provenance provenance)
        {
            var names = new Names(Straatnamen.Select(x => new StreetNameName(x.Value, x.Key.ToLanguage())));
            return new CorrectStreetNameNames(municipalityId, new PersistentLocalId(PersistentLocalId), names, provenance);
        }
    }
}
