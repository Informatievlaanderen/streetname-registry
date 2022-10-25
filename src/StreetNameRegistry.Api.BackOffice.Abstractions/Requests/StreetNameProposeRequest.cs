namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Convertors;
    using MediatR;
    using Municipality;
    using Municipality.Commands;
    using Newtonsoft.Json;
    using Response;

    [DataContract(Name = "VoorstelStraatnaam", Namespace = "")]
    public class StreetNameProposeRequest : ProposeStreetNameBackOfficeRequest, IRequest<PersistentLocalIdETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to ProposeStreetName command
        /// </summary>
        /// <returns>ProposeStreetName.</returns>
        public ProposeStreetName ToCommand(MunicipalityId municipalityId, Provenance provenance, PersistentLocalId persistentLocalId)
        {
            var names = new Names(Straatnamen.Select(x => new StreetNameName(x.Value, x.Key.ToLanguage())));
            return new ProposeStreetName(municipalityId, names, persistentLocalId, provenance);
        }
    }
}
