namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;

    [DataContract(Name = "CorrigerenStraatnaamHomoniemToevoegingen", Namespace = "")]
    public sealed class CorrectStreetNameHomonymAdditionsRequest
    {
        /// <summary>
        /// De homoniemtoevoegingen in de officiële taal en faciliteitentaal van de gemeente.
        /// </summary>
        [DataMember(Name = "Homoniemtoevoegingen", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public Dictionary<Taal, string> HomoniemToevoegingen { get; set; }
    }
}
