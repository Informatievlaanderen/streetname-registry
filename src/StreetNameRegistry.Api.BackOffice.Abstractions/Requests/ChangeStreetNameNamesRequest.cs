namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;

    [DataContract(Name = "WijzigenStraatnaamNamen", Namespace = "")]
    public sealed class ChangeStreetNameNamesRequest
    {
        /// <summary>
        /// De straatnaam in elke officiële taal en faciliteitentaal van de gemeente.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public Dictionary<Taal, string> Straatnamen { get; set; }
    }
}
